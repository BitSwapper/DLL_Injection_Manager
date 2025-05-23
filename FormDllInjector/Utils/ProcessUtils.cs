using FormDllInjector.Native;
using InjectorCommon.Native;

namespace FormDllInjector.Utils;

public static class ProcessUtils
{
    public enum ProcessBitness
    {
        Unknown,
        Bit32,
        Bit64
    }

    public static ProcessBitness GetTargetProcessBitness(int processId)
    {
        if(!Environment.Is64BitOperatingSystem)
            return ProcessBitness.Bit32;

        nint processHandle = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_INFORMATION,
                                false, processId);
        if(processHandle == nint.Zero)
        {
            return ProcessBitness.Unknown;
        }

        try
        {
            if(!NativeMethods_Form.IsWow64Process(processHandle, out bool isWow64))
            {
                return ProcessBitness.Unknown;
            }
            return isWow64 ? ProcessBitness.Bit32 : ProcessBitness.Bit64;
        }
        finally
        {
            NativeMethods.CloseHandle(processHandle);
        }
    }
}