// KeysM.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "KeysM.h"
#include "resource.h"

#define MAX_LOADSTRING 100
#define WS_EX_NOACTIVATE        0x08000000L
#define GOLD_DUST_Q 140		//���������� ������� ����
#define GRAY_DUST_Q	8000	//���������� ����� ���� �� ����
#define BTN_DUST_Q	400		//���������� ���� ��� ������
#define _PI_ 3.14159265


//������
#define CTRL_BTN 0		//����������� - �������
#define CURSOR_BTN 1	//��������� ������ - �������
#define WND_BTN 2	//������ ���� - ������� ��� ������
#define NUM_BTN 3	//������ ����� - �����
#define OPS_BTN 4	//������ �������� -�����

typedef unsigned long MYPARAM, UNALIGNED *UNALPMYPARAM;

typedef struct
{
	HWND	hWnd;	//���������� ����
	HBITMAP hMask;	//����� ��������
	int iLeft;	// ��������� ����
	int iTop;
	DWORD dwKey;	//������ ��� ��� �������
	DWORD dwStateBtn;//��������� ������: 0 - ��������� 1-mouseover 2-mousedown 3 mouseup 4 mouseleave
	DWORD dwCounter;//������� ����� ������
	DWORD RsrcID;	//�� �������-��������
	DWORD dwGroup;	//����� ������ �������
} MYWINDATA, *PMYWINDATA;

//*******************************************************
// Global Variables:
HINSTANCE hInst;								// current instance
WCHAR szTitle[MAX_LOADSTRING];							
//��������� �������� ����
HBITMAP hFonBmp,	//������ ����, ����������� �� �������
	hDustBmp;	//������ ���� � �����,������������ ��� ��� ��� ��������� ������ ��������.
HBITMAP hOldBmp,	//������ ������ ��� ����
	hOldDustBmp;	//������ ������ ��� ���� � �����
HDC hdcFonMem,		//�������� ����
	hdcDustMem;		//�������� ���� � �����
//��������� ����������
HDC hdcBar;	//������ ������
HBITMAP hMaskBmp,	//�����
	hBarBmp,		//������� ������
	hOldBarBmp;
//����� ���������� ��������
BOOL Visual;	//��������� �� ������������ ���������� �������
DWORD	StateWnd;		//0 - �������� ��� 1- ���� ��� 2-���� ��� ����� � ���� ����� ���������� ������� 3-���� �� ������
DWORD RotatePos;	//������� �������� ���������� ������
DWORD RotatePosActive;	//������� �������� �������� ������
DWORD ActiveButtonID;	//�� �������� ������ (������� ����)


COLORREF Golds[8];	//������ ������ ������ ��� ���������
//��� ������
//������ ������ ������
COLORREF BtnColors[5][3];	//������ ������ ��� ������ 
//������� ����� Golds[1]
HBITMAP hBmpGold,		//������ ��� ������� ��������
		hOldBmpGold;	//������ ������
HDC hdcGold;		//�������� �����������
MYWINDATA myWinData[28];	//������ ������ ��� ������

DWORD TimerCounter;	//������� ��� �������� �������
HWND hOtherWnd, hMyWnd;	//����������� ���� ������ � ������

POINT CoordSet[10];	//��� ��������� �����-��������
//*******************************************************
// Foward declarations of functions included in this code module:
BOOL				MyRegisterClass(HINSTANCE hInstance);
BOOL				InitInstance(HINSTANCE, int);
LRESULT CALLBACK	WndProc(HWND, UINT, WPARAM, LPARAM);
LRESULT CALLBACK	About(HWND, UINT, WPARAM, LPARAM);
void MainWndCreate(HWND hWnd);	//�������������  �������� ����
void MainWndPaint(HWND hWnd);	//��������� �������� ����
void MainWndDestroy(HWND hWnd);	//�������� �������� ����
VOID CALLBACK TimerProc(HWND, UINT, UINT, DWORD);// ������ 
void ShowOrnament(HWND hWnd, DWORD offset); //������� ��������� ������ ���������
void ShowDust(HWND hWnd);	//������� ��������� ������� ����
void TrackMouseMain(HWND hWnd);	//������� ��������� ��������� ���� - ��� ����� ��� ���. ������ ������ �� ��������
BOOL FillColors(HDC hdcWnd);//��������� ������� ������
//BOOL CreateMonoMasks(HDC hdcWnd);//������� ����������� ��������� ��� �����
void FreeMonoMasks(HWND hWnd);//������� ����������� ��������� ��� �����
void CreateBtnParam();//��������� ������ ������ �����������
BOOL LoadMasks();//��������� ����� ������
void FreeMasks();	//��������� ����� ������
void SendKeys(BYTE VirKey);
DWORD GetWndIndex(HWND hWnd);//������� ���������� ��������������� � ����� ����� - ������ �������
BOOL  CreateMyChildWindow(HWND hParentWnd);//������� ���� � ������� � ��� ������
LRESULT CALLBACK ChildWndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
void ChildBtnPaint(HWND hWnd);
void Sets(int n, float X, float Y, float radius, float Angle);
//*******************************************************
int APIENTRY WinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPSTR     lpCmdLine,
                     int       nCmdShow)
{
 	// TODO: Place code here.
	MSG msg;

	//������ ��� ������ �� ������������
	HANDLE hMutex = CreateMutexW(NULL, TRUE, L"(C) 2008 Selyakov M Pavel (MeraMan)");
	if(GetLastError() == ERROR_ALREADY_EXISTS) {
		if(hMutex != NULL) CloseHandle(hMutex);
		return FALSE;
	};
	//� ���� ������ �� ������� ������� - ���� � ���, � ������������� - ���������� ��������

	hOtherWnd = GetForegroundWindow();	//���������� �������� ����

	// Initialize global strings
	LoadStringW(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);

	MyRegisterClass(hInstance);

	// Perform application initialization:
	if (!InitInstance (hInstance, nCmdShow)) 
	{
		return FALSE;
	}

	// Main message loop:
	while (GetMessage(&msg, NULL, 0, 0)) 
	{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
	}

	return msg.wParam;
}
//*****************************************************
//
//  FUNCTION: MyRegisterClass()
//
//  PURPOSE: Registers the window class.
//
//  COMMENTS:
//
//    This function and its usage is only necessary if you want this code
//    to be compatible with Win32 systems prior to the 'RegisterClassEx'
//    function that was added to Windows 95. It is important to call this function
//    so that the application will get 'well formed' small icons associated
//    with it.
//
BOOL MyRegisterClass(HINSTANCE hInstance)
{
	WNDCLASSEX wcex;

	wcex.cbSize = sizeof(WNDCLASSEX); 

	wcex.style			= CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc	= (WNDPROC)WndProc;
	wcex.cbClsExtra		= 0;
	wcex.cbWndExtra		= 0;
	wcex.hInstance		= hInstance;
	wcex.hIcon			= LoadIcon(hInstance, (LPCTSTR)IDI_KEYSM);
	wcex.hCursor		= LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground	= (HBRUSH)GetStockObject(BLACK_BRUSH);
	wcex.lpszMenuName	= NULL;
	wcex.lpszClassName	= L"KeysMagiClass";
	wcex.hIconSm		= LoadIcon(wcex.hInstance, (LPCTSTR)IDI_SMALL);

	if(!RegisterClassEx(&wcex)) return FALSE; //���� ������ ��� �������� ������ ����
	//����� ������ ������������
	wcex.lpfnWndProc = ChildWndProc;
	wcex.cbWndExtra = 8;	//���� ������-������ ��� �������� ������� ����� � ����
	wcex.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
	wcex.hIcon = NULL;
	wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
	wcex.hIconSm = NULL;
	wcex.lpszClassName = L"SubKeysClass";

	return (BOOL) RegisterClassEx(&wcex);
	
}
//**********************************************************************
//
//   FUNCTION: InitInstance(HANDLE, int)
//
//   PURPOSE: Saves instance handle and creates main window
//
//   COMMENTS:
//
//        In this function, we save the instance handle in a global variable and
//        create and display the main program window.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
   HWND hWnd;

   hInst = hInstance; // Store instance handle in our global variable

   hWnd = CreateWindowExW(WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_APPWINDOW, L"KeysMagiClass", szTitle, WS_CLIPCHILDREN | WS_SYSMENU | WS_MINIMIZEBOX,
      100, 100, 350, 245, NULL, NULL, hInstance, NULL);

   if (!hWnd)
   {
      return FALSE;
   }

   ShowWindow(hWnd, nCmdShow);
   UpdateWindow(hWnd);

   //������������� ���������� ����������
	//����� ������ ��������� �� ������� ��� �����
  
	TimerCounter = 0;
	hMyWnd = hWnd;	//��� ����
	RotatePos = 0;
	RotatePosActive = 0;
	ActiveButtonID = 255;

//��� ��� ������������ ����������
//	Visual = FALSE;//��� ������� ���������
//	StateWnd = 0;	//���� �� ��� �����
//� ��� ��� ����������
	Visual = TRUE;//��� ������� ���������
	StateWnd = 1;	//���� �� ��� �����

 //���� �������� ���� ����� ������, ����������� ����� ����� � ���� ����� ����������
   //������� ���� ����� ������ � ��� ������
   RECT rc;
   GetWindowRect(hWnd, &rc);
   rc.bottom = rc.bottom - rc.top;
   rc.right = rc.right - rc.left;
   rc.left = 0;
   rc.top = 0;	//���� ������ ���� � �������� �����������.
   HRGN hRgn = CreateRoundRectRgn(rc.left, rc.top, rc.right, rc.bottom, 15, 15);
   SetWindowRgn(hWnd, hRgn, TRUE);
   DeleteObject(hRgn);

   return TRUE;
}
//***********************************************************************
//
//  FUNCTION: WndProc(HWND, unsigned, WORD, LONG)
//
//  PURPOSE:  Processes messages for the main window.
//
//  WM_COMMAND	- process the application menu
//  WM_PAINT	- Paint the main window
//  WM_DESTROY	- post a quit message and return
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	int wmId, wmEvent;

	LRESULT lRes; //��� nchittest �����������


	switch (message) 
	{
		case WM_NCCALCSIZE:  //
			 return 0;	//����� �� ���������� ���� � ������� - ������ ���������� �������
	   		break;

//	case WM_NCPAINT:
//	case WM_NCACTIVATE:
//******************************************
//		case WM_NCMOUSEHOVER:			//�� ��������
//******************************************
//		case WM_NCMOUSELEAVE:			//�� ��������
//******************************************
		case WM_NCHITTEST:

			lRes = DefWindowProc(hWnd, message, wParam, lParam);
			if(lRes == HTCLIENT) {
				//���� � ���������� �������
				//��� ����-�� ������
			if(Visual) StateWnd = 2; else StateWnd = 0;
			if(ActiveButtonID != 255) myWinData[ActiveButtonID].dwStateBtn = 0;
			ActiveButtonID =  255;
				lRes = HTCAPTION; //����� ���������� �� ��� ����
			};
			return lRes;
			break;
//*******************************************
	case WM_CREATE:

		MainWndCreate(hWnd);
		return TRUE;
		break;
//******************************************	
	case WM_COMMAND:
			wmId    = LOWORD(wParam); 
			wmEvent = HIWORD(wParam); 
			// Parse the menu selections:
			switch (wmId)
			{
				case IDM_ABOUT:
				   DialogBox(hInst, (LPCTSTR)IDD_ABOUTBOX, hWnd, (DLGPROC)About);
				   break;
				case IDM_EXIT:
				   DestroyWindow(hWnd);
				   break;
				default:
				   return DefWindowProc(hWnd, message, wParam, lParam);
			}
			break;
		case WM_PAINT:
			MainWndPaint(hWnd);
			break;
		case WM_DESTROY:
			MainWndDestroy(hWnd);
			PostQuitMessage(0);
			break;
		default:
			return DefWindowProc(hWnd, message, wParam, lParam);
			break;
   }
   return 0;
}
//*****************************************
// Mesage handler for about box.
LRESULT CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
		case WM_INITDIALOG:
				return TRUE;

		case WM_COMMAND:
			if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL) 
			{
				EndDialog(hDlg, LOWORD(wParam));
				return TRUE;
			}
			break;
	}
    return FALSE;
}
//*****************************************
void MainWndPaint(HWND hWnd)
{
	PAINTSTRUCT ps;
	HDC hdc;
	RECT rt;

	hdc = BeginPaint(hWnd, &ps);

	GetClientRect(hWnd, &rt);
	BitBlt(hdc, rt.left, rt.top, rt.right - rt.left, rt.bottom - rt.top, hdcDustMem, 0, 0, SRCCOPY);

	EndPaint(hWnd, &ps);
}
//*****************************************
void MainWndCreate(HWND hWnd)
{
	HDC hdc;
	//������� �������� ����
		//��������� ������ ������
		CreateBtnParam();
		//��������� ����� ������
		LoadMasks();
		
		//��������� ��������� ��� -������� ��������
		hFonBmp = (HBITMAP) LoadImage(hInst, MAKEINTRESOURCE(IDB_FON), IMAGE_BITMAP, 0, 0, 0);
		hdc = GetDC(hWnd);
		hdcFonMem = CreateCompatibleDC(hdc);
		hOldBmp = (HBITMAP) SelectObject(hdcFonMem, hFonBmp);
		//������� �������� ��� ��������� ����
		hdcDustMem = CreateCompatibleDC(hdc);
		hDustBmp = (HBITMAP)CreateCompatibleBitmap(hdc, 350, 245);	//������� ����� ������
		hOldDustBmp = (HBITMAP)SelectObject(hdcDustMem, hDustBmp);
		BitBlt(hdcDustMem, 0, 0, 350, 245, hdcFonMem, 0, 0, SRCCOPY);//����������� � ���� ���
		//������� ������� ��� ����������
		hMaskBmp = (HBITMAP) LoadImage(hInst, MAKEINTRESOURCE(IDB_ORN1), IMAGE_BITMAP, 0, 0, 0);
		hBarBmp = (HBITMAP)CreateCompatibleBitmap(hdc, 350, 25);	//������� ������ ��� ���������
		hdcBar = CreateCompatibleDC(hdc);
		hOldBarBmp = (HBITMAP)SelectObject(hdcBar, hBarBmp);
		//������� ������� ����� Golds[0] ��� ������� ������ ��� ����������� ��������
		hBmpGold = (HBITMAP)CreateCompatibleBitmap(hdc, 30, 30);	//������� ������ ��� ���������
		hdcGold = CreateCompatibleDC(hdc);
		hOldBmpGold = (HBITMAP)SelectObject(hdcGold, hBmpGold);

		//�������� ������� ������
		FillColors(hdc);	//��������� ������������ ���������, ���� FALSE - ����� �� ���������� � �������
		ReleaseDC(hWnd, hdc);

		CreateMyChildWindow(hWnd);
		//��������� ������  ��� ��������
		SetTimer(hWnd, 1, 30, (TIMERPROC) TimerProc);
}

//**********************************************************
void MainWndDestroy(HWND hWnd)
{
		KillTimer(hWnd, 1);	//���������� ������
//������� ��������� ���
		SelectObject(hdcFonMem, hOldBmp);
		DeleteObject(hFonBmp);
		DeleteDC(hdcFonMem);
//������� ��������� ���
		SelectObject(hdcDustMem, hOldDustBmp);
		DeleteObject(hDustBmp);
		DeleteDC(hdcDustMem);
//������� ����� ���������
		DeleteObject(hMaskBmp);
//������� ������ ��� ���������
		SelectObject(hdcBar, hOldBarBmp);
		DeleteObject(hBarBmp);
		DeleteDC(hdcBar);
	
		FreeMasks();
}

//**********************************************************
void ShowDust(HWND hWnd)
{
	DWORD X, Y;
	//�������� ��� �� ������ �������� � �������� �����	
	BitBlt(hdcDustMem, 0, 0, 350, 245, hdcFonMem, 0, 0, SRCCOPY);//����������� � ���� ���
	//���� �������� ���� ���������, ������ ����
	if(StateWnd > 1) for (DWORD i = 0; i < GOLD_DUST_Q; i++) {
		X = rand() % 350;
		Y = rand() % 245;
		SetPixel(hdcDustMem, X, Y, Golds[6]);
	};
	//������
}
//*****************************************
//����� ���� ���� 
void FonDust(HWND hWnd)
{
	DWORD X, Y, cr;
	//���� �������� ���� ���������, ������ ����
	if(StateWnd > 0) for (DWORD i = 0; i < GRAY_DUST_Q; i++) {
		X = rand() % 350;
		Y = rand() % 245;
		cr = rand() & 63;
		SetPixel(hdcFonMem, X, Y, RGB(cr, cr, cr));
	};
	//������
}
//*****************************************
void ShowOrnament(HWND hWnd, DWORD offset)
{
	COLORREF cref;
	//���� �������� ������
	if(StateWnd == 0) cref = Golds[0]; else cref = Golds[1];  

	SelectObject(hdcBar, GetStockObject(DC_BRUSH));
	SetDCBrushColor(hdcBar, cref);	//��������� ���� ������ ���������
	//��������� ������ ������ ���������
	PatBlt(hdcBar, 0, 0, 350, 25, PATCOPY);
	//������ ���� ���� �� ����, �� ������ ������� �����.
	if(Visual && (StateWnd == 2)) {
		DWORD pos = offset + 1;
		SelectObject(hdcBar,GetStockObject(DC_PEN));

		do {
	//�������� ������� ������� �� ����
		SetDCPenColor(hdcBar, Golds[5]);
		SetDCBrushColor(hdcBar, Golds[7]);
		Rectangle(hdcBar, pos, 0, pos + 10, 24);

		pos += 54;	//����� ��������� ������
		} while(pos < 340);
	};	//end if
	//������ ������� ����� ����� �� ���
	MaskBlt(hdcDustMem, 0, 0, 350, 25, hdcBar, 0, 0, hMaskBmp, 0, 0, (MAKEROP4(0x00AA0029, SRCCOPY)));	//������� ������
	MaskBlt(hdcDustMem, 0, 220, 350, 25, hdcBar, 0, 0, hMaskBmp, 0, 0, (MAKEROP4(0x00AA0029, SRCCOPY)));	//������ ������
	//����� ���...

}
//**********************************************************
VOID CALLBACK TimerProc(
    HWND hwnd,        // handle to window for timer messages
    UINT message,     // WM_TIMER message
    UINT idTimer,     // timer identifier
    DWORD dwTime)     // current system time
{
		HWND tmpWnd;	
	
	TimerCounter++;
//���������� ��������� ������� � �������������� �������� ��� ��������� �������
	if((TimerCounter & 15) == 0)	{	//�������� �� 16
		if(Visual) TrackMouseMain(hwnd);	//��������� ��������� �������
		tmpWnd = GetForegroundWindow();	//�������� ����������
		// ���� ��� ���� � ��� �� ���� ����, �� ����� ��� � ����������
		if((IsWindow(tmpWnd)) && (tmpWnd != hMyWnd)) hOtherWnd = tmpWnd;
	};
//������ ���, ����� � ������� ���� 
	if(Visual && ((TimerCounter & 3) == 0))	FonDust(hwnd);	//������ ����� ���� �� ��������� ��������
	if((TimerCounter  & 1) == 0) {	//�������� �� 2
	ShowDust(hwnd);	//�������  ���� 
		//��������
	if((Visual) && (StateWnd > 1)) if(RotatePos < 355) RotatePos += 5; else RotatePos = 0;
	};

//������ ��������
	ShowOrnament(hwnd, (TimerCounter & 7)*8);
	if((Visual) && (StateWnd > 1)) if(RotatePosActive < 355) RotatePosActive += 5; else RotatePosActive = 0;

	//������������ ����
	InvalidateRect(hwnd, NULL, FALSE);
	UpdateWindow(hwnd);

	//������������ ������, ���������� - ����� ���
	
	if(Visual)  {
		for(int i = 0; i < 28; i++) {
			if((myWinData[i].dwCounter == 0) && ((TimerCounter  & 1) == 1)) continue;
			InvalidateRect(myWinData[i].hWnd, NULL, FALSE);
			UpdateWindow(myWinData[i].hWnd);
		};
	};
return;
}
//**********************************************************
void TrackMouseMain(HWND hWnd)
{
	RECT rc;
	POINT pos;
	GetCursorPos(&pos);
	GetWindowRect(hWnd, &rc);
	if(PtInRect(&rc, pos)) {
		if(StateWnd != 3) StateWnd = 2;
	} else StateWnd = 1;

}
//*********************************************************
//��������� ������� ������
BOOL FillColors(HDC hdcWnd)
{
	Golds[0] = RGB(77, 73, 49);
	Golds[1] = RGB(121, 115, 49);
	Golds[2] = RGB(157, 126, 47);
	Golds[3] = RGB(203, 180, 46);
	Golds[4] = RGB(219, 203, 87);
	Golds[5] = RGB(245, 226, 118);
	Golds[6] = RGB(249, 236, 164);
	Golds[7] = RGB(252, 255, 255);
//������� �����������
	BtnColors[CTRL_BTN][0] = RGB(107, 56, 65);
	BtnColors[CTRL_BTN][1] = RGB(175, 46, 58);
	BtnColors[CTRL_BTN][2] = RGB(247, 81, 94);
//������ ���������
	BtnColors[CURSOR_BTN][0] = Golds[1];
	BtnColors[CURSOR_BTN][1] = Golds[3];
	BtnColors[CURSOR_BTN][2] = Golds[6];
//������� - ����
	BtnColors[WND_BTN][0] = RGB(63, 79, 57);
	BtnColors[WND_BTN][1] = RGB(68, 168, 75);
	BtnColors[WND_BTN][2] = RGB(80, 255, 88);
//����� - �����
	BtnColors[NUM_BTN][0] = RGB(72, 72, 72);
	BtnColors[NUM_BTN][1] = RGB(126, 126, 126);
	BtnColors[NUM_BTN][2] = RGB(199, 199, 199);
//����� - ��������
	BtnColors[OPS_BTN][0] = RGB(63, 94, 107);
	BtnColors[OPS_BTN][1] = RGB(64, 84, 196);
	BtnColors[OPS_BTN][2] = RGB(35,	92, 245);

	return TRUE;
}
//*********************************************************
//*********************************************************

//��������� ������ ������ �����������
void CreateBtnParam()
{
	DWORD index;
	DWORD X, Y;

	X = 25;
	index = 0;
	for(int i = 1; i < 8; i++) {
		Y = 40;
		for (int j = 1; j < 5; j++)	{
			myWinData[index].dwCounter = 0;
			myWinData[index].dwStateBtn = 0;
			myWinData[index].hMask = NULL;
			myWinData[index].hWnd = NULL;
			myWinData[index].iLeft = X;
			myWinData[index].iTop = Y;
			index++;
			Y += 45;
		};	
		X += 45;
	};
	//esc
	myWinData[0].dwKey = 27;
	myWinData[0].RsrcID = IDB_ESC;
	myWinData[0].dwGroup = CTRL_BTN;
//back
	myWinData[1].dwKey = 8;
	myWinData[1].RsrcID = IDB_BACK;
	myWinData[1].dwGroup = CTRL_BTN;
//del
	myWinData[2].dwKey = 0x2E;
	myWinData[2].RsrcID = IDB_DEL;
	myWinData[2].dwGroup = CTRL_BTN;
//enter
	myWinData[3].dwKey = 0x0D;
	myWinData[3].RsrcID = IDB_ENTER;
	myWinData[3].dwGroup = CTRL_BTN;
//7
	myWinData[4].dwKey = 0x37;
	myWinData[4].RsrcID = IDB_7;
	myWinData[4].dwGroup = NUM_BTN;
//4
	myWinData[5].dwKey = 0x34;
	myWinData[5].RsrcID = IDB_4;
	myWinData[5].dwGroup = NUM_BTN;
//1
	myWinData[6].dwKey = 0x31;
	myWinData[6].RsrcID = IDB_1;
	myWinData[6].dwGroup = NUM_BTN;
//0
	myWinData[7].dwKey = 0x30;
	myWinData[7].RsrcID = IDB_0;
	myWinData[7].dwGroup = NUM_BTN;
//8
	myWinData[8].dwKey = 0x38;
	myWinData[8].RsrcID = IDB_8;
	myWinData[8].dwGroup = NUM_BTN;
//5
	myWinData[9].dwKey = 0x35;
	myWinData[9].RsrcID = IDB_5;
	myWinData[9].dwGroup = NUM_BTN;
//2
	myWinData[10].dwKey = 0x32;
	myWinData[10].RsrcID = IDB_2;
	myWinData[10].dwGroup = NUM_BTN;
//space
	myWinData[11].dwKey = 0x20;
	myWinData[11].RsrcID = IDB_SPACE;
	myWinData[11].dwGroup = NUM_BTN;
//9
	myWinData[12].dwKey = 0x39;
	myWinData[12].RsrcID = IDB_9;
	myWinData[12].dwGroup = NUM_BTN;
//6
	myWinData[13].dwKey = 0x36;
	myWinData[13].RsrcID = IDB_6;
	myWinData[13].dwGroup = NUM_BTN;
//3
	myWinData[14].dwKey = 0x33;
	myWinData[14].RsrcID = IDB_3;
	myWinData[14].dwGroup = NUM_BTN;
//comma
	myWinData[15].dwKey = 0x6E;
	myWinData[15].RsrcID = IDB_COMMA;
	myWinData[15].dwGroup = NUM_BTN;
//*
	myWinData[16].dwKey = 0x6A;
	myWinData[16].RsrcID = IDB_MUL;
	myWinData[16].dwGroup = OPS_BTN;
//-
	myWinData[17].dwKey = 0x6D;
	myWinData[17].RsrcID = IDB_MINUS;
	myWinData[17].dwGroup = OPS_BTN;
//tab
	myWinData[18].dwKey = 0x09;
	myWinData[18].RsrcID = IDB_TAB;
	myWinData[18].dwGroup = OPS_BTN;
//left
	myWinData[19].dwKey = 0x25;
	myWinData[19].RsrcID = IDB_LEFT;
	myWinData[19].dwGroup = CURSOR_BTN;
//	/
	myWinData[20].dwKey = 0x6F;
	myWinData[20].RsrcID = IDB_DIV;
	myWinData[20].dwGroup = OPS_BTN;
// +
	myWinData[21].dwKey = 0x6B;
	myWinData[21].RsrcID = IDB_PLUS;
	myWinData[21].dwGroup = OPS_BTN;
//up
	myWinData[22].dwKey = 0x26;
	myWinData[22].RsrcID = IDB_UP;
	myWinData[22].dwGroup = CURSOR_BTN;
//down
	myWinData[23].dwKey = 0x28;
	myWinData[23].RsrcID = IDB_DOWN;
	myWinData[23].dwGroup = CURSOR_BTN;
//collapse
	myWinData[24].dwKey = 0xFF;
	myWinData[24].RsrcID = IDB_COLL;
	myWinData[24].dwGroup = WND_BTN;
//exit
	myWinData[25].dwKey = 0xFE;
	myWinData[25].RsrcID = IDB_CLOSE;
	myWinData[25].dwGroup = WND_BTN;
//info
	myWinData[26].dwKey = 0xFD;
	myWinData[26].RsrcID = IDB_ABOUT;
	myWinData[26].dwGroup = WND_BTN;
//right
	myWinData[27].dwKey = 0x27;
	myWinData[27].RsrcID = IDB_RIGHT;
	myWinData[27].dwGroup = CURSOR_BTN;

}
//*********************************************************
//��������� ����� ������ 
BOOL LoadMasks()
{
	HBITMAP hBmp;
	for(int i = 0; i < 28; i++) {
		hBmp = (HBITMAP) LoadImage(hInst, MAKEINTRESOURCE(myWinData[i].RsrcID), IMAGE_BITMAP, 0, 0, 0);
		if(hBmp == NULL) return FALSE;
		myWinData[i].hMask = hBmp;
	};
	return TRUE;
}
//*********************************************************
//��������� ����� ������ 
void FreeMasks()
{
	HBITMAP hBmp;
	for(int i = 0; i < 28; i++) {
		hBmp = myWinData[i].hMask; 
		if(hBmp != NULL) DeleteObject(hBmp);
	};
	return;
}
//**********************************************************

//***************************************
LRESULT CALLBACK ChildWndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
//	PAINTSTRUCT ps;
	HDC hdc;	// htmpdc;
	UNALPMYPARAM up;
	DWORD index;
	DWORD tmp;


	switch (message)
	{
	//	case WM_NCCALCSIZE:  //CAPTURE THIS MESSAGE AND RETURN NULL
//	case WM_MOUSEMOVE:
//********************************
	case WM_CREATE:

		hdc = GetDC(hWnd);
		//������� ��� ����� ���� - ������ � �������
		//������ ��� � ���� �������...
		up = (UNALPMYPARAM) (((LPCREATESTRUCT) lParam)->lpCreateParams);
		index = *up;
		//���������, ���� ����������
		SetLastError(0);
		SetWindowLong(hWnd, GWL_USERDATA, index);
		if(GetLastError()) return FALSE;
		ReleaseDC(hWnd, hdc);

		return TRUE;
		break;
//*********************************

		case WM_NCHITTEST:
			index = GetWndIndex(hWnd);
			if(Visual) StateWnd = 3; else StateWnd = 0;	//���� ��� �������
			ActiveButtonID = index;
			//������� ������ ������ �� hover, ���� �� ����� ��� ���� ���������
			if(myWinData[index].dwStateBtn == 0) {
				RotatePosActive = RotatePos;	//���� �������� ���������� �� ������ ���� ��������	
				myWinData[index].dwStateBtn = 1;
			};
			return DefWindowProc(hWnd, message, wParam, lParam);
	//*****************************
	case WM_LBUTTONDOWN:
			SetForegroundWindow(hOtherWnd);
			index = GetWndIndex(hWnd);
			myWinData[index].dwStateBtn = 2;
			myWinData[index].dwCounter = 5;
			return DefWindowProc(hWnd, message, wParam, lParam);
		break;
//***********************
	case WM_LBUTTONUP:
			index = GetWndIndex(hWnd);

			myWinData[index].dwStateBtn = 3;
			myWinData[index].dwCounter = 5;

			tmp = myWinData[index].dwKey;	//��� �������
			switch (tmp) {
				case 0xFF:
				//�������� ����
					CloseWindow(hMyWnd);	//��� ���������� ������� ����� - 19.09.2007
					break;
				case 0xFE:
				//������� ����
					PostMessage(hMyWnd, WM_COMMAND, (DWORD) IDM_EXIT, 0);
					break;
				case 0xFD:
				//������� �����
					SendMessage(hMyWnd, WM_COMMAND, (DWORD) IDM_ABOUT, 0);
					break;
				default:
					SendKeys((BYTE) tmp);
					break;
			};

			return DefWindowProc(hWnd, message, wParam, lParam);
			break;
//***************************
		case WM_PAINT:

			ChildBtnPaint(hWnd);

			break;
//**************************
		case WM_DESTROY:

			PostQuitMessage(0);
			break;
//*************************
		default:
			return DefWindowProc(hWnd, message, wParam, lParam);
	}
   return 0;
}

//*******************************************
//������� �������� �������
void SendKeys(BYTE VirKey)
{
//	keybd_event(VirKey, 0, 0, 0);			//������ ������
//	keybd_event(VirKey, 0, KEYEVENTF_KEYUP, 0); //�������� ��������
/*		������ ��� �������
	INPUT inp[2];	//������ �������

	inp[0].type = INPUT_KEYBOARD;
	inp[1].type = INPUT_KEYBOARD;
	inp[0].ki.wVk = VirKey;
	inp[1].ki.wVk = VirKey;

	inp[0].ki.dwExtraInfo = 0;
	inp[0].ki.dwFlags = 0;
	inp[0].ki.time = 0;
	inp[0].ki.wScan = 0;

	inp[1].ki.dwExtraInfo = 0;
	inp[1].ki.dwFlags = KEYEVENTF_KEYUP;
	inp[1].ki.time = 0;
	inp[1].ki.wScan = 0;

UINT res = SendInput(2, inp, sizeof(inp[0]));
*/
}

//*********************************************
//������� ���� � ������� � ��� ������
BOOL  CreateMyChildWindow(HWND hParentWnd)
{
	int index = 0;
	HWND hChildWnd;
	MYPARAM MyParam;
	UNALPMYPARAM up;
	BOOL RetVal = TRUE;

	for(index = 0; index < 28; index++)
	{
		MyParam = index;
		up = &MyParam;

//������� ����
		hChildWnd = CreateWindowEx(0,	//�����
			L"SubKeysClass",			//�����
			L"Text",						//��������
			WS_CHILD | WS_VISIBLE, // �����
			myWinData[index].iLeft,				// x
			myWinData[index].iTop,				// y
			30,				//������
			30,				//������
			hParentWnd,			//����� ����
			NULL,			//����
			hInst,			//����������
			up);			//�������� ��� ������

		if(!hChildWnd) {
			RetVal = FALSE;
			break;
		};
	//������� ���������� ���� � ������, ����� ����� ������������
	myWinData[index].hWnd = hChildWnd;

	};	//end for

	return RetVal;
}

//****************************************
//������� ���������� ��������������� � ����� ����� - ������ �������
DWORD GetWndIndex(HWND hWnd)
{
	return GetWindowLong(hWnd, GWL_USERDATA);
}

//*****************************************
void ChildBtnPaint(HWND hWnd)
{
DWORD index;
PAINTSTRUCT ps;
HDC hdc;
DWORD BtnGroup, BtnColor, BtnState, BtnCounter;
float Angle;
DWORD Num, i, j;
COLORREF color;

			index = GetWndIndex(hWnd);
			BtnGroup = myWinData[index].dwGroup;
			BtnState = myWinData[index].dwStateBtn;
			BtnCounter = myWinData[index].dwCounter;
			if(StateWnd < 3) BtnState = 0;
			hdc = BeginPaint(hWnd, &ps);
			//��������� ���
			BitBlt(hdc, 0, 0, 30, 30, hdcDustMem, myWinData[index].iLeft, myWinData[index].iTop, SRCCOPY);

			//����� �����
			switch (BtnGroup) {
			case CTRL_BTN:
			case CURSOR_BTN:
					Num = 3;
					break;
			case  OPS_BTN:
					Num = 4;
					break;
			case NUM_BTN:
					Num = 5;
					break;
			case  WND_BTN:
					Num = 6;
					break;
			};
			//�������� ����� 
			switch	(BtnState)
			{
			case 1:		// mouse over
				BtnColor = 1;

				color = Golds[2];
				Angle = (float) RotatePosActive;

				break;
			case 2:			//mousedown
				BtnColor = 2;
				color = Golds[3];
				Num = 10;
				Angle = 0;
				//�������� ������
				Num = 10 - BtnCounter; 
				if(BtnCounter > 0) BtnCounter--;
				break;
			case 3:		//mousedown
				BtnColor = 1;	//������������� ����� ������
				color = Golds[3];	//���� �����
				Num = 5 + BtnCounter;	//����� ����� �����
				Angle = 0;				//���� ��������
				if(BtnCounter > 0) BtnCounter--; else BtnState = 1;	//���� �������� ���������, ��������� � ��������� mouseover
				break;
			default:		//mouse leave
				color = Golds[1];
				BtnColor = 0;
				Angle = (float)RotatePos;
				break;
			}
//������ ����� ��� ��������	
//���� ���������� ����� ��������, �� ���� �������� Golds[0];
	if(!Visual) color = Golds[0];

	Sets(Num, 15, 15, 15, Angle);
	SelectObject(hdc, GetStockObject(DC_PEN));
	SetDCPenColor(hdc, color);
	//���� ������ � ��������� up down, ������������ ��� ����� ����� ������ �������
	if(BtnState < 2) {
		MoveToEx(hdc, CoordSet[0].x, CoordSet[0].y, NULL);
		for(i = 1; i < Num; i++) 	LineTo(hdc, CoordSet[i].x, CoordSet[i].y);
		LineTo(hdc, CoordSet[0].x, CoordSet[0].y);
	} else {
		for(i = 0; i < Num; i++) 
			for(j = i; j < Num; j++) {
				MoveToEx(hdc, CoordSet[i].x, CoordSet[i].y, NULL);
				LineTo(hdc, CoordSet[j].x, CoordSet[j].y);
			};
	};
//��������� �������� ������
//���� ���� ��� �� ����, �������� ��� ������ �������
//� ���� ���� �� ������ - �� ��������� ���
	SelectObject(hdcGold, GetStockObject(DC_BRUSH));
	switch(StateWnd)	{
	case 2:	//������ ������� ���� 
		SetDCBrushColor(hdcGold, BtnColors[BtnGroup][BtnColor]);
		PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);
		break;
	case 3:	//������ ���� �� �������  
		if(BtnState > 0) {	//������ �� ���������� ������ - � �����
			SetDCBrushColor(hdcGold, BtnColors[BtnGroup][BtnColor-1]);
			PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);
			for(i = 0; i < BTN_DUST_Q; i++) SetPixel(hdcGold, rand() % 28, rand() % 28, BtnColors[BtnGroup][BtnColor]);
		} else {	//������ �� ��������� ���������� ������� ��� ���� � ����
			SetDCBrushColor(hdcGold, BtnColors[BtnGroup][BtnColor]);
			PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);
		};
		break;
	default:
		SetDCBrushColor(hdcGold, Golds[0]);
		PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);
		break;
	}

//� ���� ��� ������ �������������, �� ���������� ����� ������ �������
//	������� �� ������ - ���� ����� �������
//	SetDCBrushColor(hdcGold, Golds[0]);
//	PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);

	//�������� ������ �� ������ ��������� ������ ����� ��� �����
	MaskBlt(hdc, 0, 0, 30, 30, hdcGold, 0, 0, myWinData[index].hMask, 0, 0, (MAKEROP4(0x00AA0029, SRCCOPY)));	//������� ������
			//����� �� �����
	EndPaint(hWnd, &ps);
	myWinData[index].dwStateBtn = BtnState;	//�������� ����� ��������� ������
	myWinData[index].dwCounter = BtnCounter;	//�������� ����� ������� ������
}


//*****************************************************
void Line(HDC hdc, int fromX, int fromY, int toX, int toY)
{
	MoveToEx(hdc, fromX, fromY, NULL);
	LineTo(hdc, toX, toY);
}

void Sets(int n, float X, float Y, float radius, float Angle)
{
	double Ang = Angle * _PI_ / 180; //���� ���������
	double Step = 2 * _PI_ / n;	//��� ����
	for(int i = 0; i < n; i++) {
		Ang += Step;
	CoordSet[i].x = (long) radius * cos(Ang) + X;
	CoordSet[i].y = (long) radius * sin(Ang) + Y;
	};
	return;
}

