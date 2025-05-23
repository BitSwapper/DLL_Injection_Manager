namespace InjectorCommon.Statics;

public static class StaticInjectorStrings
{
    public static readonly string Kernel32DllName = "kernel32.dll";
    public static readonly string LoadLibraryFunctionName = "LoadLibraryA";
    public static readonly string ErrorMsgOpenProcessFailed = "Failed to open process (PID: {0}). Error code: {1}";
    public static readonly string ErrorMsgGetLoadLibraryAddressFailed = "Failed to get address of " + LoadLibraryFunctionName + ". Error code: {0}";
    public static readonly string ErrorMsgGetKernel32HandleFailed = "Failed to get handle for " + Kernel32DllName + ". Error code: {0}";
    public static readonly string ErrorMsgAllocateMemoryFailed = "Failed to allocate memory in target process. Error code: {0}";
    public static readonly string ErrorMsgWriteMemoryFailed = "Failed to write DLL path to process memory. Error code: {0}";
    public static readonly string ErrorMsgCreateRemoteThreadFailed = "Failed to create remote thread. Error code: {0}";
    public static readonly string ErrorMsgGetExitCodeFailed = "Failed to get remote thread exit code. Error code: {0}";
    public static readonly string ErrorMsgLoadLibraryFailedInTarget = LoadLibraryFunctionName + " failed in the target process (returned NULL or zero). Check DLL (e.g. bitness, dependencies, DllMain).";
    public static readonly string ErrorMsgRemoteThreadTimedOut = "Remote thread timed out (" + LoadLibraryFunctionName + " took too long or deadlocked).";
    public static readonly string ErrorMsgWaitForThreadFailed = "Waiting for remote thread failed. Wait result: {0}. Error code: {1}";
    public static readonly string ErrorMsgGenericException = "Generic Exception during injection: {0}";
    public static readonly string SuccessMsgDllInjected = "DLL injected successfully.";
}