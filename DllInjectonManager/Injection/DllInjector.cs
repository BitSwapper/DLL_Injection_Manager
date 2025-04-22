using System.Runtime.InteropServices;
using System.Text;

namespace AddyTools.DllInjectonManager.Injection;

public class DllInjector
{
    const string Kernel32DllName = "kernel32.dll";
    const string LoadLibraryFunctionName = "LoadLibraryA";
    const uint StandardProcessAccess = NativeFunctions.PROCESS_CREATE_THREAD | NativeFunctions.PROCESS_QUERY_INFORMATION | NativeFunctions.PROCESS_VM_OPERATION | NativeFunctions.PROCESS_VM_WRITE | NativeFunctions.PROCESS_VM_READ;
    const uint MemoryAllocationType = NativeFunctions.MEM_COMMIT | NativeFunctions.MEM_RESERVE;
    const uint MemoryProtection = NativeFunctions.PAGE_READWRITE;
    const uint MemoryReleaseType = NativeFunctions.MEM_RELEASE;
    const uint DefaultStackSize = 0;
    const uint DefaultCreationFlags = 0;
    const uint InjectThreadTimeoutMilliseconds = 5000;
    const bool InheritHandles = false;
    const string CStyleNullTerminator = "\0";
    static readonly string ErrorMsgOpenProcessFailed = "Failed to open process (PID: {0}). Error code: {1}";
    static readonly string ErrorMsgGetLoadLibraryAddressFailed = "Failed to get address of " + LoadLibraryFunctionName + ". Error code: {0}";
    static readonly string ErrorMsgAllocateMemoryFailed = "Failed to allocate memory in target process. Error code: {0}";
    static readonly string ErrorMsgWriteMemoryFailed = "Failed to write DLL path to process memory. Error code: {0}";
    static readonly string ErrorMsgCreateRemoteThreadFailed = "Failed to create remote thread. Error code: {0}";
    static readonly string ErrorMsgGetExitCodeFailed = "Failed to get remote thread exit code. Error code: {0}";
    static readonly string ErrorMsgLoadLibraryFailedInTarget = LoadLibraryFunctionName + " failed in the target process (returned NULL). Check DLL dependencies, path, bitness, and DllMain.";
    static readonly string ErrorMsgRemoteThreadTimedOut = "Remote thread timed out (" + LoadLibraryFunctionName + " took too long or deadlocked).";
    static readonly string ErrorMsgWaitForThreadFailed = "Waiting for remote thread failed. Wait result: {0}. Error code: {1}";
    static readonly string ErrorMsgGenericException = "Generic Exception during injection: {0}";
    static readonly string SuccessMsgDllInjected = "DLL injected successfully.";

    public (bool Success, string Message, nint ModuleHandle) InjectDll(int processId, string dllPath)
    {
        nint hProcess = nint.Zero;
        nint pDllPath = nint.Zero;
        nint hThread = nint.Zero;
        nint moduleHandle = nint.Zero;

        try
        {
            hProcess = NativeFunctions.OpenProcess(StandardProcessAccess, InheritHandles, processId);
            if(hProcess == nint.Zero)
                return (false, string.Format(ErrorMsgOpenProcessFailed, processId, Marshal.GetLastWin32Error()), nint.Zero);

            nint hKernel32 = NativeFunctions.GetModuleHandle(Kernel32DllName);
            if(hKernel32 == nint.Zero)
                return (false, $"Failed to get handle for {Kernel32DllName}. Error code: {Marshal.GetLastWin32Error()}", nint.Zero);

            nint loadLibraryAddr = NativeFunctions.GetProcAddress(hKernel32, LoadLibraryFunctionName);
            if(loadLibraryAddr == nint.Zero)
                return (false, string.Format(ErrorMsgGetLoadLibraryAddressFailed, Marshal.GetLastWin32Error()), nint.Zero);

            byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath + CStyleNullTerminator);
            uint bufferSize = (uint)dllPathBytes.Length;

            pDllPath = NativeFunctions.VirtualAllocEx(hProcess, nint.Zero, bufferSize, MemoryAllocationType, MemoryProtection);
            if(pDllPath == nint.Zero)
                return (false, string.Format(ErrorMsgAllocateMemoryFailed, Marshal.GetLastWin32Error()), nint.Zero);

            if(!NativeFunctions.WriteProcessMemory(hProcess, pDllPath, dllPathBytes, bufferSize, out nint bytesWritten) || bytesWritten.ToInt32() != bufferSize)
            {
                NativeFunctions.VirtualFreeEx(hProcess, pDllPath, nuint.Zero, MemoryReleaseType);
                pDllPath = nint.Zero;
                return (false, string.Format(ErrorMsgWriteMemoryFailed, Marshal.GetLastWin32Error()), nint.Zero);
            }

            hThread = NativeFunctions.CreateRemoteThread(hProcess, nint.Zero, DefaultStackSize, loadLibraryAddr, pDllPath, DefaultCreationFlags, out _);
            if(hThread == nint.Zero)
            {
                NativeFunctions.VirtualFreeEx(hProcess, pDllPath, nuint.Zero, MemoryReleaseType);
                pDllPath = nint.Zero;
                return (false, string.Format(ErrorMsgCreateRemoteThreadFailed, Marshal.GetLastWin32Error()), nint.Zero);
            }

            uint waitResult = NativeFunctions.WaitForSingleObject(hThread, InjectThreadTimeoutMilliseconds);

            if(waitResult == NativeFunctions.WAIT_OBJECT_0)
            {
                if(!NativeFunctions.GetExitCodeThread(hThread, out moduleHandle))
                    return (false, string.Format(ErrorMsgGetExitCodeFailed, Marshal.GetLastWin32Error()), nint.Zero);

                if(moduleHandle == nint.Zero)
                    return (false, ErrorMsgLoadLibraryFailedInTarget, nint.Zero);

                MessageBox.Show("Placed at " + pDllPath.ToString("X"));
                return (true, SuccessMsgDllInjected, moduleHandle);
            }
            else if(waitResult == NativeFunctions.WAIT_TIMEOUT)
                return (false, ErrorMsgRemoteThreadTimedOut, nint.Zero);
            else
                return (false, string.Format(ErrorMsgWaitForThreadFailed, waitResult, Marshal.GetLastWin32Error()), nint.Zero);
        }
        catch(Exception ex)
        {
            return (false, string.Format(ErrorMsgGenericException, ex.Message), nint.Zero);
        }
        finally
        {
            if(hThread != nint.Zero)
                NativeFunctions.CloseHandle(hThread);

            if(pDllPath != nint.Zero && hProcess != nint.Zero)
                NativeFunctions.VirtualFreeEx(hProcess, pDllPath, nuint.Zero, MemoryReleaseType);

            if(hProcess != nint.Zero)
                NativeFunctions.CloseHandle(hProcess);
        }
    }
}