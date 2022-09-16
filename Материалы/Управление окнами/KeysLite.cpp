// KeysLite.cpp : Defines the entry point for the application.
//
#define UNICODE
#include "stdafx.h"

#define MY_TIMER_EVENT 1022
#define WS_EX_NOACTIVATE        0x08000000L

HINSTANCE hI;
HWND hOtherWnd, hMyWnd;	//дескрипторы окон чужого и своего
DWORD Top, Left;	//позици€ окна


//for minimize crt
#pragma comment(linker,"/MERGE:.rdata=.text")
#pragma comment(linker,"/FILEALIGN:512 /SECTION:.text,EWRX /IGNORE:4078")
#pragma comment(linker,"/ENTRY:New_WinMain")
#pragma comment(linker,"/NODEFAULTLIB")

//-----------------------------------------

VOID CALLBACK TimerProcedure(
    HWND hwnd,        // handle to window for timer messages
    UINT message,     // WM_TIMER message
    UINT idTimer,     // timer identifier
    DWORD dwTime)     // current system time
{
HWND tmpWnd = GetForegroundWindow();	//получаем форегроунд
// если это окно и это не наше окно, то пишем его в переменную
if((IsWindow(tmpWnd)) && (tmpWnd != hMyWnd)) hOtherWnd = tmpWnd;

return;
}

//-----------------------------------------

//функци€ устанавливает иконки дл€ кнопок
void InitButtons(HWND hwnd)
{
DWORD ids[28][2];
ids[0][0] = IDI_IESC;
ids[0][1] = IDC_BESC;
ids[1][0] = IDI_IBACK;
ids[1][1] = IDC_BBACK;
ids[2][0] = IDI_IDEL;
ids[2][1] = IDC_BDEL;
ids[3][0] = IDI_IENT;
ids[3][1] = IDC_BENTER;
ids[4][0] = IDI_I0;
ids[4][1] = IDC_B0;
ids[5][0] = IDI_I000; 
ids[5][1] = IDC_B000;
ids[6][0] = IDI_ICOMM; 
ids[6][1] = IDC_BCOMMA;
ids[7][0] = IDI_I1;
ids[7][1] = IDC_B1;
ids[8][0] = IDI_I2; 
ids[8][1] = IDC_B2;
ids[9][0] = IDI_I3; 
ids[9][1] = IDC_B3;
ids[10][0] = IDI_I4;
ids[10][1] = IDC_B4;
ids[11][0] = IDI_I5;
ids[11][1] = IDC_B5;
ids[12][0] = IDI_I6; 
ids[12][1] = IDC_B6;
ids[13][0] = IDI_I7;
ids[13][1] = IDC_B7;
ids[14][0] = IDI_I8;
ids[14][1] = IDC_B8;
ids[15][0] = IDI_I9; 
ids[15][1] = IDC_B9;
ids[16][0] = IDI_IUP;
ids[16][1] = IDC_BUP;
ids[17][0] = IDI_IDOWN;
ids[17][1] = IDC_BDOWN;
ids[18][0] = IDI_ILEFT;
ids[18][1] = IDC_BLEFT;
ids[19][0] = IDI_IRIGHT;
ids[19][1] = IDC_BRIGHT;
ids[20][0] = IDI_IDIV;
ids[20][1] = IDC_BDIV;
ids[21][0] = IDI_IMUL;
ids[21][1] = IDC_BMUL;
ids[22][0] = IDI_IMINUS;
ids[22][1] = IDC_BMINUS;
ids[23][0] = IDI_IPLUS;
ids[23][1] = IDC_BPLUS;
ids[24][0] = IDI_IPERC;
ids[24][1] = IDC_BPERC;
ids[25][0] = IDI_ISPACE;
ids[25][1] = IDC_BSPACE;
ids[26][0] = IDI_IEQU;
ids[26][1] = IDC_BEQU;
ids[27][0] = IDI_ITAB;
ids[27][1] = IDC_BTAB;

HICON hicon;
//загружаем иконки на кнопки
for(int i = 0; i < 28; i++) {
hicon = LoadIcon(hI, MAKEINTRESOURCE(ids[i][0]));
SendMessage(GetDlgItem(hwnd, ids[i][1]), (UINT) BM_SETIMAGE, (WPARAM) IMAGE_ICON, (LPARAM) hicon);
//пам€ть под иконками освобождать будет винда при закрытии окошка
}// end for
}
//----------------------------------------------

//*************************************
//грузить настройки текущего пользовател€ из реестра.
// если ключи не созданы - вернуть FALSE
BOOL LoadRegistrySetting(LPDWORD lpTop, LPDWORD lpLeft)
{
	HKEY hKey;
	DWORD lRes;

	if(RegOpenKeyExW(HKEY_CURRENT_USER, L"Software\\MeraMan\\Keys", 0, KEY_QUERY_VALUE, &hKey) != ERROR_SUCCESS) return FALSE;
	lRes = 4;

	if((RegQueryValueExW(hKey, L"Top", NULL, NULL, (LPBYTE) lpTop, &lRes) !=  ERROR_SUCCESS) || (RegQueryValueExW(hKey, L"Left", NULL, NULL, (LPBYTE) lpLeft, &lRes) !=  ERROR_SUCCESS)) {
		RegCloseKey(hKey);
		return FALSE;
	};
	return TRUE;
}


//*************************************

BOOL SaveRegistrySetting(LPDWORD lpTop, LPDWORD lpLeft)
{
	HKEY hKey;


	if(RegOpenKeyExW(HKEY_CURRENT_USER, L"Software\\MeraMan\\Keys", 0, KEY_SET_VALUE, &hKey) != ERROR_SUCCESS) return FALSE;


	if((RegSetValueExW(hKey, L"Top", 0, REG_DWORD, (BYTE *) lpTop, sizeof(DWORD)) !=  ERROR_SUCCESS) || (RegSetValueExW(hKey, L"Left", 0, REG_DWORD, (BYTE *) lpLeft, sizeof(DWORD)) !=  ERROR_SUCCESS)) {
		RegCloseKey(hKey);
		return FALSE;
	};
	return TRUE;
}
//----------------------------------------------
// dialog init
BOOL OnInitDialog(HWND hwnd)
{
//сбить визуальный стиль с диалога
RECT rc;
GetWindowRect(hwnd, &rc);
rc.right = rc.right - rc.left;
rc.bottom = rc.bottom - rc.top;
SetWindowRgn(hwnd, CreateRectRgn(0,0,rc.right, rc.bottom), TRUE); 
DWORD style = GetWindowLong(hwnd, GWL_EXSTYLE);
SetWindowLong(hwnd, GWL_EXSTYLE, style | WS_EX_TOPMOST | WS_EX_NOACTIVATE | WS_EX_APPWINDOW);
	
SetWindowPos(hwnd, HWND_TOPMOST, Left, Top, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE);
// присвоить кнопкам иконки
InitButtons(hwnd);
hMyWnd = hwnd;	//установим дескриптор нашего окна в глобальную переменную
SetTimer(hwnd, MY_TIMER_EVENT, 300, (TIMERPROC) TimerProcedure);

return TRUE;
}

//----------------------------------------
//функци€ посылает клавишу
void SendKeys(BYTE VirKey, DWORD Flags)
{
//	keybd_event(VirKey, 0, 0, 0);			//нажмем кнопку
//	keybd_event(VirKey, 0, KEYEVENTF_KEYUP, 0); //отпустим кнопочку

	INPUT inp;	//событие

	inp.type = INPUT_KEYBOARD;
	inp.ki.wVk = VirKey;
	inp.ki.dwExtraInfo = 0;
	inp.ki.dwFlags = Flags;
	inp.ki.time = 0;
	inp.ki.wScan = 0;


	SendInput(1, &inp, sizeof(INPUT));
}

void SingleKeys(BYTE VirtKey)
{
	SendKeys(VirtKey, 0);
	SendKeys(VirtKey, KEYEVENTF_KEYUP);
}

void ExitDlg(HWND hwnd) 
{
	//если это обновление - записать положение окна в реестр
	//если демка- не надо записывать
	
	if(!IsIconic(hwnd)) {
	RECT rc;
	GetWindowRect(hwnd, &rc);
	Top = rc.top;
	Left = rc.left;
	SaveRegistrySetting(&Top, &Left);
	}
	//таймер выключить
	KillTimer(hwnd, MY_TIMER_EVENT);

}

//----------------------------------------

BOOL OnCommand(HWND hwnd, int CtlType, int CtlID, HWND hWndCtrl)
{
	switch (CtlID) {
	case IDOK:
	case IDCANCEL:
		ExitDlg(hwnd);	//очистка ресурсов и сохранение позиции
		EndDialog(hwnd, CtlID);
		break;
	case IDC_B000:
		SingleKeys(0x30);
		SingleKeys(0x30);
		SingleKeys(0x30);
		break;
// в демке эти части заменить на вызов MessageBox с текстом о демо-версии
	case IDC_B0:
		SingleKeys(0x30);
		break;
	case IDC_B1:
		SingleKeys(0x31);
		break;
	case IDC_B4:
		SingleKeys(0x34);
		break;
	case IDC_B7:
		SingleKeys(0x37);
		break;
//конец демо-участка
	case IDC_B2:
		SingleKeys(0x32);
		break;
	case IDC_B3:
		SingleKeys(0x33);
		break;
	case IDC_B5:
		SingleKeys(0x35);
		break;
	case IDC_B6:
		SingleKeys(0x36);
		break;
	case IDC_B8:
		SingleKeys(0x38);
		break;
	case IDC_B9:
		SingleKeys(0x39);
		break;
	case IDC_BCOMMA:
		SingleKeys(0x6E);
		break;
	case IDC_BESC:
		SingleKeys(27);
		break;
	case IDC_BBACK:
		SingleKeys(0x08);
		break;
	case IDC_BDEL:
		SingleKeys(0x2E);
		break;
	case IDC_BENTER:
		SingleKeys(0x0D);
		break;
	case IDC_BSPACE:
		SingleKeys(0x20);
		break;
	case IDC_BMUL:
		SingleKeys(0x6A);
		break;
	case IDC_BMINUS:
		SingleKeys(0x6D);
		break;
	case IDC_BTAB:
		SingleKeys(0x09);
		break;
	case IDC_BLEFT:
		SingleKeys(0x25);
		break;
	case IDC_BDIV:
		SingleKeys(0x6F);
		break;
	case IDC_BPLUS:
		SingleKeys(0x6B);
		break;
	case IDC_BUP:
		SingleKeys(0x26);
		break;
	case IDC_BDOWN:
		SingleKeys(0x28);
		break;
	case IDC_BRIGHT:
		SingleKeys(0x27);
		break;
	case IDC_BEQU:
		SingleKeys(0xBB);
		break;
	case IDC_BPERC:
		SendKeys(VK_SHIFT, 0);
		SingleKeys(0x35);
		SendKeys(VK_SHIFT, KEYEVENTF_KEYUP);
		break;
	} //end switch
	return FALSE;
}

//---------------------------------------------------

int CALLBACK DlgProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
{
	switch (msg)
	{

	case WM_INITDIALOG:
		return OnInitDialog(hwnd);
		break;
	case WM_COMMAND:
		return OnCommand(hwnd, HIWORD(wparam), LOWORD(wparam), (HWND) lparam);
		break;
	case WM_NCHITTEST:
		SetForegroundWindow(hOtherWnd);
		break;
  default:	
	break;
	}
return FALSE;
}

//----------------------------------------------------

void New_WinMain(void)
{
 hI = GetModuleHandle(NULL);

//мутекс дл€ защиты от дублировани€
	HANDLE hMutex = CreateMutexW(NULL, TRUE, L"(C) 2007 Selyakov M Pavel (MeraMan)");
	if(GetLastError() == ERROR_ALREADY_EXISTS) {
		if(hMutex != NULL) CloseHandle(hMutex);
		return;
	};
	//а если мутекс не удалось создать - хрен с ним, с дублированием - продолжаем работать
	//запоминаем переднее окно
	hOtherWnd = GetForegroundWindow();

	if(!LoadRegistrySetting(&Top, &Left)) {
		//нет ключей в реестре - дл€ версии-обновлени€ - выход с мессагой,
		// дл€ демки - продолжаем работать, а топ и лефт - по умолчанию = 0
		MessageBoxW(NULL, L"»звин€йте, не установлены мы.", L"ќшибка",MB_OK | MB_ICONSTOP);
		return;
	}

	DialogBox(hI, MAKEINTRESOURCE(IDD_DIALOG1), NULL, DlgProc);
	return;
}



