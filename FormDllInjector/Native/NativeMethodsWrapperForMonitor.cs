using System.Text;

namespace FormDllInjector.Native;

public class NativeMethodsWrapperForMonitor : INativeMethodsProvider
{
    public nint OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId) => InjectorCommon.Native.NativeMethods.OpenProcess(dwDesiredAccess, bInheritHandle, dwProcessId);

    public bool CloseHandle(nint hObject) => InjectorCommon.Native.NativeMethods.CloseHandle(hObject);

    public bool EnumProcessModulesEx(nint hProcess, nint[] lphModule, uint cb, out uint lpcbNeeded, uint dwFilterFlag) => NativeMethods_Form.EnumProcessModulesEx(hProcess, lphModule, cb, out lpcbNeeded, dwFilterFlag);

    public uint GetModuleFileNameExW(nint hProcess, nint hModule, StringBuilder lpFilename, uint nSize) => NativeMethods_Form.GetModuleFileNameExW(hProcess, hModule, lpFilename, nSize);
}