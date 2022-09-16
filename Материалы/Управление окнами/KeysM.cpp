// KeysM.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "KeysM.h"
#include "resource.h"

#define MAX_LOADSTRING 100
#define WS_EX_NOACTIVATE        0x08000000L
#define GOLD_DUST_Q 140		//количество золотой пыли
#define GRAY_DUST_Q	8000	//количество серой пыли на фоне
#define BTN_DUST_Q	400		//количество пыли для кнопки
#define _PI_ 3.14159265


//группы
#define CTRL_BTN 0		//управляющие - красные
#define CURSOR_BTN 1	//курсорные кнопки - зеленые
#define WND_BTN 2	//кнопки окна - золотые или желтые
#define NUM_BTN 3	//кнопки чисел - серые
#define OPS_BTN 4	//кнопки операции -синие

typedef unsigned long MYPARAM, UNALIGNED *UNALPMYPARAM;

typedef struct
{
	HWND	hWnd;	//дескриптор окна
	HBITMAP hMask;	//маска картинки
	int iLeft;	// положение окна
	int iTop;
	DWORD dwKey;	//символ или код клавиши
	DWORD dwStateBtn;//состояние кнопки: 0 - неактивна 1-mouseover 2-mousedown 3 mouseup 4 mouseleave
	DWORD dwCounter;//счетчик тиков кнопки
	DWORD RsrcID;	//ИД ресурса-картинки
	DWORD dwGroup;	//номер группы клавиши
} MYWINDATA, *PMYWINDATA;

//*******************************************************
// Global Variables:
HINSTANCE hInst;								// current instance
WCHAR szTitle[MAX_LOADSTRING];							
//рисование главного окна
HBITMAP hFonBmp,	//битмап фона, загруженный из ресурса
	hDustBmp;	//битмап фона с пылью,используемый как фон для рисования других объектов.
HBITMAP hOldBmp,	//старый битмап для фона
	hOldDustBmp;	//старый битмап для фона с пылью
HDC hdcFonMem,		//контекст фона
	hdcDustMem;		//контекст фона с пылью
//рисование орнаментов
HDC hdcBar;	//желтая полоса
HBITMAP hMaskBmp,	//маска
	hBarBmp,		//бегущая полоса
	hOldBarBmp;
//флаги визуальных эффектов
BOOL Visual;	//разрешены ли регистрацией визуальные эффекты
DWORD	StateWnd;		//0 - эффектов нет 1- мыши нет 2-мышь над окном и окно может отображать эффекты 3-мышь на кнопке
DWORD RotatePos;	//позиция вращения неактивных кнопок
DWORD RotatePosActive;	//позиция вращения активной кнопки
DWORD ActiveButtonID;	//ИД активной кнопки (имеющей мышь)


COLORREF Golds[8];	//массив цветов золота для элементов
//для кнопок
//массив цветов кнопок
COLORREF BtnColors[5][3];	//массив цветов для кнопок 
//квадрат цвета Golds[1]
HBITMAP hBmpGold,		//битмап для заливки паутинок
		hOldBmpGold;	//старый битмап
HDC hdcGold;		//контекст монохромный
MYWINDATA myWinData[28];	//массив данных для кнопок

DWORD TimerCounter;	//счетчик для делителя таймера
HWND hOtherWnd, hMyWnd;	//дескрипторы окон чужого и своего

POINT CoordSet[10];	//для рисования рамки-паутинки
//*******************************************************
// Foward declarations of functions included in this code module:
BOOL				MyRegisterClass(HINSTANCE hInstance);
BOOL				InitInstance(HINSTANCE, int);
LRESULT CALLBACK	WndProc(HWND, UINT, WPARAM, LPARAM);
LRESULT CALLBACK	About(HWND, UINT, WPARAM, LPARAM);
void MainWndCreate(HWND hWnd);	//инициализация  главного окна
void MainWndPaint(HWND hWnd);	//рисование главного окна
void MainWndDestroy(HWND hWnd);	//удаление главного окна
VOID CALLBACK TimerProc(HWND, UINT, UINT, DWORD);// таймер 
void ShowOrnament(HWND hWnd, DWORD offset); //функция добавляет полосы орнамента
void ShowDust(HWND hWnd);	//функция добавляет золотую пыль
void TrackMouseMain(HWND hWnd);	//функция проверяет положение мыши - над окном или нет. другие методы не работают
BOOL FillColors(HDC hdcWnd);//заполняем массивы цветов
//BOOL CreateMonoMasks(HDC hdcWnd);//создаем монохромные контексты для масок
void FreeMonoMasks(HWND hWnd);//удаляем монохромные контексты для масок
void CreateBtnParam();//заполняем массив кнопок параметрами
BOOL LoadMasks();//загрузить маски кнопок
void FreeMasks();	//выгрузить маски кнопок
void SendKeys(BYTE VirKey);
DWORD GetWndIndex(HWND hWnd);//функция возвращает ассоциированное с окном число - индекс массива
BOOL  CreateMyChildWindow(HWND hParentWnd);//создаем окна и заносим в них данные
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

	//мутекс для защиты от дублирования
	HANDLE hMutex = CreateMutexW(NULL, TRUE, L"(C) 2008 Selyakov M Pavel (MeraMan)");
	if(GetLastError() == ERROR_ALREADY_EXISTS) {
		if(hMutex != NULL) CloseHandle(hMutex);
		return FALSE;
	};
	//а если мутекс не удалось создать - хрен с ним, с дублированием - продолжаем работать

	hOtherWnd = GetForegroundWindow();	//запоминаем переднее окно

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

	if(!RegisterClassEx(&wcex)) return FALSE; //если ошибка при создании класса окна
	//класс кнопки регистрируем
	wcex.lpfnWndProc = ChildWndProc;
	wcex.cbWndExtra = 8;	//байт экстра-памяти для хранения индекса прямо в окне
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

   //инициализация глобальных переменных
	//часть данных загрузить из реестра или файла
  
	TimerCounter = 0;
	hMyWnd = hWnd;	//мое окно
	RotatePos = 0;
	RotatePosActive = 0;
	ActiveButtonID = 255;

//так при незарегенном рпиложении
//	Visual = FALSE;//виз эффекты запрещены
//	StateWnd = 0;	//мышь не над окном
//а так при зарегенном
	Visual = TRUE;//виз эффекты запрещены
	StateWnd = 1;	//мышь не над окном

 //тута присвоим окну новый регион, единственно чтобы сбить с него стиль оформления
   //который сука лезет наружу и все портит
   RECT rc;
   GetWindowRect(hWnd, &rc);
   rc.bottom = rc.bottom - rc.top;
   rc.right = rc.right - rc.left;
   rc.left = 0;
   rc.top = 0;	//тута размер окна в экранных координатах.
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

	LRESULT lRes; //для nchittest обработчика


	switch (message) 
	{
		case WM_NCCALCSIZE:  //
			 return 0;	//чтобы не показывать меню и границы - только клиентскую область
	   		break;

//	case WM_NCPAINT:
//	case WM_NCACTIVATE:
//******************************************
//		case WM_NCMOUSEHOVER:			//не работает
//******************************************
//		case WM_NCMOUSELEAVE:			//не работает
//******************************************
		case WM_NCHITTEST:

			lRes = DefWindowProc(hWnd, message, wParam, lParam);
			if(lRes == HTCLIENT) {
				//мыша в клиентской области
				//тут чего-то делаем
			if(Visual) StateWnd = 2; else StateWnd = 0;
			if(ActiveButtonID != 255) myWinData[ActiveButtonID].dwStateBtn = 0;
			ActiveButtonID =  255;
				lRes = HTCAPTION; //чтобы перемещать за все окно
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
	//создать дочерние окна
		//заполняем массив кнопок
		CreateBtnParam();
		//загружаем маски кнопок
		LoadMasks();
		
		//загружаем первичный фон -готовим контекст
		hFonBmp = (HBITMAP) LoadImage(hInst, MAKEINTRESOURCE(IDB_FON), IMAGE_BITMAP, 0, 0, 0);
		hdc = GetDC(hWnd);
		hdcFonMem = CreateCompatibleDC(hdc);
		hOldBmp = (HBITMAP) SelectObject(hdcFonMem, hFonBmp);
		//готовим контекст для рисования пыли
		hdcDustMem = CreateCompatibleDC(hdc);
		hDustBmp = (HBITMAP)CreateCompatibleBitmap(hdc, 350, 245);	//создали новый битмап
		hOldDustBmp = (HBITMAP)SelectObject(hdcDustMem, hDustBmp);
		BitBlt(hdcDustMem, 0, 0, 350, 245, hdcFonMem, 0, 0, SRCCOPY);//скопировали в него фон
		//готовим ресурсы для орнаментов
		hMaskBmp = (HBITMAP) LoadImage(hInst, MAKEINTRESOURCE(IDB_ORN1), IMAGE_BITMAP, 0, 0, 0);
		hBarBmp = (HBITMAP)CreateCompatibleBitmap(hdc, 350, 25);	//создали битмап для орнамента
		hdcBar = CreateCompatibleDC(hdc);
		hOldBarBmp = (HBITMAP)SelectObject(hdcBar, hBarBmp);
		//создаем квадрат цвета Golds[0] для заливки кнопок при выключенных эффектах
		hBmpGold = (HBITMAP)CreateCompatibleBitmap(hdc, 30, 30);	//создали битмап для орнамента
		hdcGold = CreateCompatibleDC(hdc);
		hOldBmpGold = (HBITMAP)SelectObject(hdcGold, hBmpGold);

		//заполним массивы цветов
		FillColors(hdc);	//проверить возвращенный результат, если FALSE - выйти из приложения с ошибкой
		ReleaseDC(hWnd, hdc);

		CreateMyChildWindow(hWnd);
		//установим таймер  для анимации
		SetTimer(hWnd, 1, 30, (TIMERPROC) TimerProc);
}

//**********************************************************
void MainWndDestroy(HWND hWnd)
{
		KillTimer(hWnd, 1);	//уничтожить таймер
//удалить первичный фон
		SelectObject(hdcFonMem, hOldBmp);
		DeleteObject(hFonBmp);
		DeleteDC(hdcFonMem);
//удалить вторичный фон
		SelectObject(hdcDustMem, hOldDustBmp);
		DeleteObject(hDustBmp);
		DeleteDC(hdcDustMem);
//удалить маску орнамента
		DeleteObject(hMaskBmp);
//удалить полосу для орнамента
		SelectObject(hdcBar, hOldBarBmp);
		DeleteObject(hBarBmp);
		DeleteDC(hdcBar);
	
		FreeMasks();
}

//**********************************************************
void ShowDust(HWND hWnd)
{
	DWORD X, Y;
	//копируем фон во второй контекст и посыпаем пылью	
	BitBlt(hdcDustMem, 0, 0, 350, 245, hdcFonMem, 0, 0, SRCCOPY);//скопировали в него фон
	//если анимация окна разрешена, рисуем пыль
	if(StateWnd > 1) for (DWORD i = 0; i < GOLD_DUST_Q; i++) {
		X = rand() % 350;
		Y = rand() % 245;
		SetPixel(hdcDustMem, X, Y, Golds[6]);
	};
	//готово
}
//*****************************************
//серая пыль фона 
void FonDust(HWND hWnd)
{
	DWORD X, Y, cr;
	//если анимация окна разрешена, рисуем пыль
	if(StateWnd > 0) for (DWORD i = 0; i < GRAY_DUST_Q; i++) {
		X = rand() % 350;
		Y = rand() % 245;
		cr = rand() & 63;
		SetPixel(hdcFonMem, X, Y, RGB(cr, cr, cr));
	};
	//готово
}
//*****************************************
void ShowOrnament(HWND hWnd, DWORD offset)
{
	COLORREF cref;
	//цвет согласно режиму
	if(StateWnd == 0) cref = Golds[0]; else cref = Golds[1];  

	SelectObject(hdcBar, GetStockObject(DC_BRUSH));
	SetDCBrushColor(hdcBar, cref);	//установим цвет полосы орнамента
	//заполняем полосу цветом орнамента
	PatBlt(hdcBar, 0, 0, 350, 25, PATCOPY);
	//теперь если мышь на окне, то рисуем светлую волну.
	if(Visual && (StateWnd == 2)) {
		DWORD pos = offset + 1;
		SelectObject(hdcBar,GetStockObject(DC_PEN));

		do {
	//отрисуем светлые полоски на баре
		SetDCPenColor(hdcBar, Golds[5]);
		SetDCBrushColor(hdcBar, Golds[7]);
		Rectangle(hdcBar, pos, 0, pos + 10, 24);

		pos += 54;	//место следующей полосы
		} while(pos < 340);
	};	//end if
	//теперь наложим через маску на фон
	MaskBlt(hdcDustMem, 0, 0, 350, 25, hdcBar, 0, 0, hMaskBmp, 0, 0, (MAKEROP4(0x00AA0029, SRCCOPY)));	//верхняя полоса
	MaskBlt(hdcDustMem, 0, 220, 350, 25, hdcBar, 0, 0, hMaskBmp, 0, 0, (MAKEROP4(0x00AA0029, SRCCOPY)));	//нижняя полоса
	//вроде все...

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
//определяем положение курсора и соответственно включаем или выключаем эффекты
	if((TimerCounter & 15) == 0)	{	//делитель на 16
		if(Visual) TrackMouseMain(hwnd);	//проверяем положение курсора
		tmpWnd = GetForegroundWindow();	//получаем форегроунд
		// если это окно и это не наше окно, то пишем его в переменную
		if((IsWindow(tmpWnd)) && (tmpWnd != hMyWnd)) hOtherWnd = tmpWnd;
	};
//рисуем фон, серую и золотую пыль 
	if(Visual && ((TimerCounter & 3) == 0))	FonDust(hwnd);	//рисуем серую пыль на первичной картинке
	if((TimerCounter  & 1) == 0) {	//делитель на 2
	ShowDust(hwnd);	//золотая  пыль 
		//вращение
	if((Visual) && (StateWnd > 1)) if(RotatePos < 355) RotatePos += 5; else RotatePos = 0;
	};

//рисуем орнамент
	ShowOrnament(hwnd, (TimerCounter & 7)*8);
	if((Visual) && (StateWnd > 1)) if(RotatePosActive < 355) RotatePosActive += 5; else RotatePosActive = 0;

	//перерисовать окно
	InvalidateRect(hwnd, NULL, FALSE);
	UpdateWindow(hwnd);

	//перерисовать кнопки, неактивные - через раз
	
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
//заполняем массивы цветов
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
//красные управляющие
	BtnColors[CTRL_BTN][0] = RGB(107, 56, 65);
	BtnColors[CTRL_BTN][1] = RGB(175, 46, 58);
	BtnColors[CTRL_BTN][2] = RGB(247, 81, 94);
//желтые курсорные
	BtnColors[CURSOR_BTN][0] = Golds[1];
	BtnColors[CURSOR_BTN][1] = Golds[3];
	BtnColors[CURSOR_BTN][2] = Golds[6];
//зеленые - окна
	BtnColors[WND_BTN][0] = RGB(63, 79, 57);
	BtnColors[WND_BTN][1] = RGB(68, 168, 75);
	BtnColors[WND_BTN][2] = RGB(80, 255, 88);
//серые - числа
	BtnColors[NUM_BTN][0] = RGB(72, 72, 72);
	BtnColors[NUM_BTN][1] = RGB(126, 126, 126);
	BtnColors[NUM_BTN][2] = RGB(199, 199, 199);
//синие - операции
	BtnColors[OPS_BTN][0] = RGB(63, 94, 107);
	BtnColors[OPS_BTN][1] = RGB(64, 84, 196);
	BtnColors[OPS_BTN][2] = RGB(35,	92, 245);

	return TRUE;
}
//*********************************************************
//*********************************************************

//заполняем массив кнопок параметрами
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
//загрузить маски кнопок 
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
//выгрузить маски кнопок 
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
		//получим наш номер окна - индекс в массиве
		//делаем как в мсдн сказано...
		up = (UNALPMYPARAM) (((LPCREATESTRUCT) lParam)->lpCreateParams);
		index = *up;
		//проверить, чего получилось
		SetLastError(0);
		SetWindowLong(hWnd, GWL_USERDATA, index);
		if(GetLastError()) return FALSE;
		ReleaseDC(hWnd, hdc);

		return TRUE;
		break;
//*********************************

		case WM_NCHITTEST:
			index = GetWndIndex(hWnd);
			if(Visual) StateWnd = 3; else StateWnd = 0;	//мышь над кнопкой
			ActiveButtonID = index;
			//сменить статус кнопки на hover, если до этого она была неактивна
			if(myWinData[index].dwStateBtn == 0) {
				RotatePosActive = RotatePos;	//угол вращения начинается от общего угла вращения	
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

			tmp = myWinData[index].dwKey;	//код клавиши
			switch (tmp) {
				case 0xFF:
				//свернуть окно
					CloseWindow(hMyWnd);	//так правильнее наверно будет - 19.09.2007
					break;
				case 0xFE:
				//закрыть окно
					PostMessage(hMyWnd, WM_COMMAND, (DWORD) IDM_EXIT, 0);
					break;
				case 0xFD:
				//вызвать абоут
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
//функция посылает клавишу
void SendKeys(BYTE VirKey)
{
//	keybd_event(VirKey, 0, 0, 0);			//нажмем кнопку
//	keybd_event(VirKey, 0, KEYEVENTF_KEYUP, 0); //отпустим кнопочку
/*		только для отладки
	INPUT inp[2];	//массив событий

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
//создаем окна и заносим в них данные
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

//создаем окно
		hChildWnd = CreateWindowEx(0,	//стиль
			L"SubKeysClass",			//класс
			L"Text",						//Название
			WS_CHILD | WS_VISIBLE, // стили
			myWinData[index].iLeft,				// x
			myWinData[index].iTop,				// y
			30,				//ширина
			30,				//высота
			hParentWnd,			//родит окно
			NULL,			//меню
			hInst,			//приложение
			up);			//параметр для креате

		if(!hChildWnd) {
			RetVal = FALSE;
			break;
		};
	//занесем дескриптор окна в массив, чтобы потом использовать
	myWinData[index].hWnd = hChildWnd;

	};	//end for

	return RetVal;
}

//****************************************
//функция возвращает ассоциированное с окном число - индекс массива
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
			//скопируем фон
			BitBlt(hdc, 0, 0, 30, 30, hdcDustMem, myWinData[index].iLeft, myWinData[index].iTop, SRCCOPY);

			//число углов
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
			//нарисуем цифру 
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
				//анимация кнопок
				Num = 10 - BtnCounter; 
				if(BtnCounter > 0) BtnCounter--;
				break;
			case 3:		//mousedown
				BtnColor = 1;	//интенсивность цвета кнопки
				color = Golds[3];	//цвет рамки
				Num = 5 + BtnCounter;	//число углов рамки
				Angle = 0;				//угол поворота
				if(BtnCounter > 0) BtnCounter--; else BtnState = 1;	//если анимация закончена, вернуться к состоянию mouseover
				break;
			default:		//mouse leave
				color = Golds[1];
				BtnColor = 0;
				Angle = (float)RotatePos;
				break;
			}
//рисуем рамку или паутинку	
//если визуальный стиль выключен, то цвет паутинки Golds[0];
	if(!Visual) color = Golds[0];

	Sets(Num, 15, 15, 15, Angle);
	SelectObject(hdc, GetStockObject(DC_PEN));
	SetDCPenColor(hdc, color);
	//если кнопка в состоянии up down, отрисовываем доп линии иначе только границы
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
//рисование символов кнопок
//если мыши нет на окне, рисовать все кнопки золотым
//а если мышь на кнопке - то добавлять шум
	SelectObject(hdcGold, GetStockObject(DC_BRUSH));
	switch(StateWnd)	{
	case 2:	//рисуем блеклый цвет 
		SetDCBrushColor(hdcGold, BtnColors[BtnGroup][BtnColor]);
		PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);
		break;
	case 3:	//рисуем цвет на кнопках  
		if(BtnState > 0) {	//рисуем на выделенной кнопке - с шумом
			SetDCBrushColor(hdcGold, BtnColors[BtnGroup][BtnColor-1]);
			PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);
			for(i = 0; i < BTN_DUST_Q; i++) SetPixel(hdcGold, rand() % 28, rand() % 28, BtnColors[BtnGroup][BtnColor]);
		} else {	//рисуем на остальных неактивных кнопках без шума и пыли
			SetDCBrushColor(hdcGold, BtnColors[BtnGroup][BtnColor]);
			PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);
		};
		break;
	default:
		SetDCBrushColor(hdcGold, Golds[0]);
		PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);
		break;
	}

//а если эти строки раскомментить, то рисоваться будет только золотом
//	золотое на черном - тоже очень красиво
//	SetDCBrushColor(hdcGold, Golds[0]);
//	PatBlt(hdcGold, 0, 0, 30, 30, PATCOPY);

	//выводиим значок на кнопку выбранным цветом через его маску
	MaskBlt(hdc, 0, 0, 30, 30, hdcGold, 0, 0, myWinData[index].hMask, 0, 0, (MAKEROP4(0x00AA0029, SRCCOPY)));	//верхняя полоса
			//вывод на экран
	EndPaint(hWnd, &ps);
	myWinData[index].dwStateBtn = BtnState;	//сохраним новое состояние кнопки
	myWinData[index].dwCounter = BtnCounter;	//сохраним новый счетчик кнопки
}


//*****************************************************
void Line(HDC hdc, int fromX, int fromY, int toX, int toY)
{
	MoveToEx(hdc, fromX, fromY, NULL);
	LineTo(hdc, toX, toY);
}

void Sets(int n, float X, float Y, float radius, float Angle)
{
	double Ang = Angle * _PI_ / 180; //угол начальный
	double Step = 2 * _PI_ / n;	//шаг угла
	for(int i = 0; i < n; i++) {
		Ang += Step;
	CoordSet[i].x = (long) radius * cos(Ang) + X;
	CoordSet[i].y = (long) radius * sin(Ang) + Y;
	};
	return;
}

