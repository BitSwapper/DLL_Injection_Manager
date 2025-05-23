using InjectorCommon.Utils;

namespace InjectorCommon;

public static class Injector
{
    public static InjectionResult Inject(int targetProcessId, string dllPath)
    {
        if(string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
            return new InjectionResult { Success = false, Message = $"DLL not found or path invalid: {dllPath}", ModuleHandle = 0L };

        ProcessBitness currentBitness = ProcessUtils.GetCurrentProcessBitness();
        ProcessBitness targetBitness = ProcessUtils.GetProcessBitness(targetProcessId);

        if(targetBitness == ProcessBitness.Unknown)
            return new InjectionResult { Success = false, Message = $"Could not determine bitness of target process ID: {targetProcessId}. Injection aborted.", ModuleHandle = 0L };

        if(currentBitness != targetBitness)
        {
            return new InjectionResult
            {
                Success = false,
                Message = $"Bitness mismatch: Current process is {currentBitness}, target process is {targetBitness}. " +
                          $"Direct injection requires matching bitness. Use a {targetBitness} helper process.",
                ModuleHandle = 0L
            };
        }

        return InjectorCommon.Core.InjectorCore.InjectDllInternal(targetProcessId, dllPath);
    }
}