#define WINVER 0x0500
#define _WIN32_WINNT 0x0500

#include <stdio.h>
#include <Windows.h>
using namespace std;

DWORD WINAPI Hide_Mouse(LPVOID lpParam)
{
	ShowCursor(NULL);
	return TRUE;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD _reason, LPVOID lpReserved)
{
	if (_reason == DLL_PROCESS_ATTACH)
	{
		CreateThread(0, 0x1000, &Hide_Mouse, 0, 0, NULL);
	}
	
	return TRUE;
}
