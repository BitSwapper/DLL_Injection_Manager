﻿using InjectorCommon.Native;
using InjectorCommon.Utils;

namespace FormDllInjector.Utils;
public static class ProcessUtils
{
    public static ProcessBitness GetTargetProcessBitness(int processId)
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
        finally
        {
            if(processHandle != nint.Zero)
                NativeMethods.CloseHandle(processHandle);
        }
    }
}