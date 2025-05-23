using System.Runtime.InteropServices;
using System.Text;
using static DLL_Injection_Manager.DllInjectonManager.Constants.Constants_Strings_DllInjector;

namespace DLL_Injection_Manager.DllInjectonManager.Injection; public class DllInjector
{
    const bool InheritHandles = false;
    const uint DefaultStackSize = 0;
    const uint DefaultCreationFlags = 0;
    const uint InjectThreadTimeoutMilliseconds = 5000;
    const uint MemoryReleaseType = NativeFunctions.MEM_RELEASE;
    const uint MemoryProtection = NativeFunctions.PAGE_READWRITE;
    const uint MemoryAllocationType = NativeFunctions.MEM_COMMIT | NativeFunctions.MEM_RESERVE;
    const uint StandardProcessAccess = NativeFunctions.PROCESS_CREATE_THREAD | NativeFunctions.PROCESS_QUERY_INFORMATION | NativeFunctions.PROCESS_VM_OPERATION | NativeFunctions.PROCESS_VM_WRITE | NativeFunctions.PROCESS_VM_READ;

    public (bool Success, string Message, nint ModuleHandle) InjectDll(int processId, string dllPath)
    {
        bool success = false;
        string message = string.Empty;
        nint threadHandle = nint.Zero;
        nint moduleHandle = nint.Zero;
        nint processHandle = nint.Zero;
        nint dllPathPointer = nint.Zero;

        try
        {
            var openProcResult = TryOpenProcessHandle(processId);
            if(!openProcResult.Success) return (false, openProcResult.Message, nint.Zero);
            processHandle = openProcResult.ProcessHandle;

            var loadLibAddrResult = TryGetLoadLibraryAddress();
            if(!loadLibAddrResult.Success) return (false, loadLibAddrResult.Message, nint.Zero);
            nint loadLibraryAddr = loadLibAddrResult.Address;

            var writePathResult = TryAllocateAndWritePath(processHandle, dllPath);
            if(!writePathResult.Success) return (false, writePathResult.Message, nint.Zero);
            dllPathPointer = writePathResult.PathAddress;

            var createThreadResult = TryCreateInjectionThread(processHandle, loadLibraryAddr, dllPathPointer);
            if(!createThreadResult.Success) return (false, createThreadResult.Message, nint.Zero);
            threadHandle = createThreadResult.ThreadHandle;

            var waitResult = WaitForThreadAndGetResult(threadHandle);
            success = waitResult.Success;
            message = waitResult.Message;
            moduleHandle = waitResult.ModuleHandle;

            if(success)
                MessageBox.Show("DLL Path placed in target memory at: 0x" + dllPathPointer.ToString("X"));

            return (success, message, moduleHandle);
        }
        catch(Exception ex)
        {
            return (false, string.Format(ErrorMsgGenericException, ex.Message), nint.Zero);
        }
        finally
        {
            if(threadHandle != nint.Zero)
                NativeFunctions.CloseHandle(threadHandle);

            if(dllPathPointer != nint.Zero && processHandle != nint.Zero)
                NativeFunctions.VirtualFreeEx(processHandle, dllPathPointer, nuint.Zero, MemoryReleaseType);

            if(processHandle != nint.Zero)
                NativeFunctions.CloseHandle(processHandle);
        }
    }

    (bool Success, nint ProcessHandle, string Message) TryOpenProcessHandle(int processId)
    {
        nint processHandle = NativeFunctions.OpenProcess(StandardProcessAccess, InheritHandles, processId);
        if(processHandle == nint.Zero)
            return (false, nint.Zero, string.Format(ErrorMsgOpenProcessFailed, processId, Marshal.GetLastWin32Error()));

        return (true, processHandle, string.Empty);
    }

    (bool Success, nint Address, string Message) TryGetLoadLibraryAddress()
    {
        nint Kernel32Handle = NativeFunctions.GetModuleHandle(Kernel32DllName);
        if(Kernel32Handle == nint.Zero)
            return (false, nint.Zero, string.Format(ErrorMsgGetKernel32HandleFailed, Kernel32DllName, Marshal.GetLastWin32Error()));

        nint loadLibraryAddy = NativeFunctions.GetProcAddress(Kernel32Handle, LoadLibraryFunctionName);
        if(loadLibraryAddy == nint.Zero)
            return (false, nint.Zero, string.Format(ErrorMsgGetLoadLibraryAddressFailed, Marshal.GetLastWin32Error()));

        return (true, loadLibraryAddy, string.Empty);
    }

    (bool Success, nint PathAddress, string Message) TryAllocateAndWritePath(nint hProcess, string dllPath)
    {
        nint dllPathPointer = nint.Zero;
        byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath + CStyleNullTerminator);
        uint bufferSize = (uint)dllPathBytes.Length;

        dllPathPointer = NativeFunctions.VirtualAllocEx(hProcess, nint.Zero, bufferSize, MemoryAllocationType, MemoryProtection);
        if(dllPathPointer == nint.Zero)
            return (false, nint.Zero, string.Format(ErrorMsgAllocateMemoryFailed, Marshal.GetLastWin32Error()));

        if(!NativeFunctions.WriteProcessMemory(hProcess, dllPathPointer, dllPathBytes, bufferSize, out nint bytesWritten) || bytesWritten.ToInt32() != bufferSize)
        {
            NativeFunctions.VirtualFreeEx(hProcess, dllPathPointer, nuint.Zero, MemoryReleaseType);
            return (false, nint.Zero, string.Format(ErrorMsgWriteMemoryFailed, Marshal.GetLastWin32Error()));
        }

        return (true, dllPathPointer, string.Empty);
    }

    (bool Success, nint ThreadHandle, string Message) TryCreateInjectionThread(nint hProcess, nint loadLibraryAddr, nint pDllPath)
    {
        nint threadHandle = NativeFunctions.CreateRemoteThread(hProcess, nint.Zero, DefaultStackSize, loadLibraryAddr, pDllPath, DefaultCreationFlags, out _);
        if(threadHandle == nint.Zero)
            return (false, nint.Zero, string.Format(ErrorMsgCreateRemoteThreadFailed, Marshal.GetLastWin32Error()));
        return (true, threadHandle, string.Empty);
    }

    (bool Success, nint ModuleHandle, string Message) WaitForThreadAndGetResult(nint threadHandle)
    {
        nint moduleHandle = nint.Zero;
        uint waitResult = NativeFunctions.WaitForSingleObject(threadHandle, InjectThreadTimeoutMilliseconds);

        if(waitResult == NativeFunctions.WAIT_OBJECT_0)
        {
            if(!NativeFunctions.GetExitCodeThread(threadHandle, out moduleHandle))
                return (false, nint.Zero, string.Format(ErrorMsgGetExitCodeFailed, Marshal.GetLastWin32Error()));

            if(moduleHandle == nint.Zero)
                return (false, nint.Zero, ErrorMsgLoadLibraryFailedInTarget);

            return (true, moduleHandle, SuccessMsgDllInjected);
        }
        else if(waitResult == NativeFunctions.WAIT_TIMEOUT)
            return (false, nint.Zero, ErrorMsgRemoteThreadTimedOut);
        else
            return (false, nint.Zero, string.Format(ErrorMsgWaitForThreadFailed, waitResult, Marshal.GetLastWin32Error()));
    }
}
