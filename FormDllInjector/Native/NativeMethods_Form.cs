using System.Runtime.InteropServices;
using System.Text;

namespace FormDllInjector.Native;

public static class NativeMethods_Form
{
    public const uint PROCESS_QUERY_INFORMATION = 0x0400;
    public const uint PROCESS_VM_READ = 0x0010;

    public const uint LIST_MODULES_ALL = 0x03;
    public const uint MAX_PATH = 260;


    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(nint hObject);

    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWow64Process([In] nint processHandle, [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool EnumProcessModulesEx(nint hProcess, [Out] nint[] lphModule, uint cb, out uint lpcbNeeded, uint dwFilterFlag);

    [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern uint GetModuleFileNameExW(nint hProcess, nint hModule, [Out] StringBuilder lpFilename, uint nSize);
}