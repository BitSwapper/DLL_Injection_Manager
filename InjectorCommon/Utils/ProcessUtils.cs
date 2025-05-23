using InjectorCommon.Native;

namespace InjectorCommon.Utils;

public enum ProcessBitness
{
    Unknown,
    Bit32,
    Bit64
}

public static class ProcessUtils
{
    public static ProcessBitness GetProcessBitness(int processId)
    {
        if(!Environment.Is64BitOperatingSystem)
            return ProcessBitness.Bit32;

        nint processHandle = nint.Zero;
        try
        {
            processHandle = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_INFORMATION, false, processId);
            if(processHandle == nint.Zero)
                return ProcessBitness.Unknown;

            if(!NativeMethods.IsWow64Process(processHandle, out bool isWow64))
                return ProcessBitness.Unknown;

            return isWow64 ? ProcessBitness.Bit32 : ProcessBitness.Bit64;
        }
        catch(Exception ex)
        {
            return ProcessBitness.Unknown;
        }
        finally
        {
            if(processHandle != nint.Zero)
                NativeMethods.CloseHandle(processHandle);
        }
    }

    public static ProcessBitness GetCurrentProcessBitness() => IntPtr.Size == 8 ? ProcessBitness.Bit64 : ProcessBitness.Bit32;
}