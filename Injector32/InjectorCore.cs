//using System;
//using System.Runtime.InteropServices;
//using System.Text;
//using InjectorCommon;
//using InjectorCommon.Native;
//using InjectorCommon.Statics;

//namespace Injector32.Core
//{
//    public static class InjectorCore
//    {
//        private const uint StandardProcessAccess = NativeMethods.PROCESS_CREATE_THREAD | NativeMethods.PROCESS_QUERY_INFORMATION | NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_VM_READ;
//        private const uint InjectThreadTimeoutMilliseconds = 5000;

//        public static InjectionResult InjectDllInternal(int processId, string dllPath)
//        {
//            nint processHandle = nint.Zero;
//            nint allocatedMemory = nint.Zero;
//            nint threadHandle = nint.Zero;

//            try
//            {
//                processHandle = NativeMethods.OpenProcess(StandardProcessAccess, false, processId);
//                if(processHandle == nint.Zero)
//                    return new InjectionResult { Success = false, Message = string.Format(StaticInjectorStrings.ErrorMsgOpenProcessFailed, processId, Marshal.GetLastWin32Error()), ModuleHandle = 0L };

//                nint loadLibraryAddr = GetLoadLibraryAddress();
//                if(loadLibraryAddr == nint.Zero)
//                    return new InjectionResult { Success = false, Message = string.Format(StaticInjectorStrings.ErrorMsgGetLoadLibraryAddressFailed, Marshal.GetLastWin32Error()), ModuleHandle = 0L };

//                byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath + '\0');
//                nuint size = (nuint)dllPathBytes.Length;

//                allocatedMemory = NativeMethods.VirtualAllocEx(processHandle, nint.Zero, size, NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE, NativeMethods.PAGE_READWRITE);
//                if(allocatedMemory == nint.Zero)
//                    return new InjectionResult { Success = false, Message = string.Format(StaticInjectorStrings.ErrorMsgAllocateMemoryFailed, Marshal.GetLastWin32Error()), ModuleHandle = 0L };

//                if(!NativeMethods.WriteProcessMemory(processHandle, allocatedMemory, dllPathBytes, size, out _))
//                {
//                    return new InjectionResult { Success = false, Message = string.Format(StaticInjectorStrings.ErrorMsgWriteMemoryFailed, Marshal.GetLastWin32Error()), ModuleHandle = 0L };
//                }

//                threadHandle = NativeMethods.CreateRemoteThread(processHandle, nint.Zero, 0, loadLibraryAddr, allocatedMemory, 0, out _);
//                if(threadHandle == nint.Zero)
//                    return new InjectionResult { Success = false, Message = string.Format(StaticInjectorStrings.ErrorMsgCreateRemoteThreadFailed, Marshal.GetLastWin32Error()), ModuleHandle = 0L };

//                uint waitResult = NativeMethods.WaitForSingleObject(threadHandle, InjectThreadTimeoutMilliseconds);
//                if(waitResult == NativeMethods.WAIT_OBJECT_0)
//                {
//                    if(NativeMethods.GetExitCodeThread(threadHandle, out nint moduleHandleExitCode))
//                    {
//                        if(moduleHandleExitCode == nint.Zero)
//                            return new InjectionResult { Success = false, Message = StaticInjectorStrings.ErrorMsgLoadLibraryFailedInTarget, ModuleHandle = 0L };

//                        return new InjectionResult { Success = true, Message = StaticInjectorStrings.SuccessMsgDllInjected, ModuleHandle = (long)moduleHandleExitCode };
//                    }
//                    return new InjectionResult { Success = false, Message = string.Format(StaticInjectorStrings.ErrorMsgGetExitCodeFailed, Marshal.GetLastWin32Error()), ModuleHandle = 0L };
//                }
//                else if(waitResult == NativeMethods.WAIT_TIMEOUT)
//                    return new InjectionResult { Success = false, Message = StaticInjectorStrings.ErrorMsgRemoteThreadTimedOut, ModuleHandle = 0L };
//                else
//                    return new InjectionResult { Success = false, Message = string.Format(StaticInjectorStrings.ErrorMsgWaitForThreadFailed, waitResult, Marshal.GetLastWin32Error()), ModuleHandle = 0L };
//            }
//            catch(Exception ex)
//            {
//                return new InjectionResult { Success = false, Message = string.Format(StaticInjectorStrings.ErrorMsgGenericException, ex.Message), ModuleHandle = 0L };
//            }
//            finally
//            {
//                if(threadHandle != nint.Zero) NativeMethods.CloseHandle(threadHandle);
//                if(allocatedMemory != nint.Zero && processHandle != nint.Zero) NativeMethods.VirtualFreeEx(processHandle, allocatedMemory, 0, NativeMethods.MEM_RELEASE);
//                if(processHandle != nint.Zero) NativeMethods.CloseHandle(processHandle);
//            }
//        }

//        private static nint GetLoadLibraryAddress()
//        {
//            nint kernel32Handle = NativeMethods.GetModuleHandle(StaticInjectorStrings.Kernel32DllName);
//            if(kernel32Handle == nint.Zero)
//            {
//                return nint.Zero;
//            }
//            nint procAddress = NativeMethods.GetProcAddress(kernel32Handle, StaticInjectorStrings.LoadLibraryFunctionName);
//            return procAddress;
//        }
//    }
//}