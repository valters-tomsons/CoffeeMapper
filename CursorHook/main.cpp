#include <stdio.h>
#include <Windows.h>
#include <iostream>
#include <fstream>
#include <string> 
using namespace std;

DWORD WINAPI Hide_Mouse(LPVOID lpParam)
{
	HCURSOR count = SetCursor(NULL);

	ofstream out;
	out.open("C:\\Users\\infin\\Desktop\\log.txt");
	out << "Number: " << count;
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
