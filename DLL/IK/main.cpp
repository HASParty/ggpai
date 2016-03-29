#include <iostream>

#define DllExport __declspec (dllexport)

extern "C" {
	DllExport int GetRandom() {
		return rand();
	}
}