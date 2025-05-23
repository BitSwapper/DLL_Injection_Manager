using System.Text;

namespace FormDllInjector.ProcessMonitoring;

public interface INativeMethodsProvider
{
    nint OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    bool CloseHandle(nint hObject);
    bool EnumProcessModulesEx(nint hProcess, nint[] lphModule, uint cb, out uint lpcbNeeded, uint dwFilterFlag);
    uint GetModuleFileNameExW(nint hProcess, nint hModule, StringBuilder lpFilename, uint nSize);
}