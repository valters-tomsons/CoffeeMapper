#include <stdio.h>
#include <Windows.h>

DWORD WINAPI Hide_Mouse(LPVOID lpParam)
{
	for (int i = 50; i > 0; i++)
	{
		SetCursor(false);
	}
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
