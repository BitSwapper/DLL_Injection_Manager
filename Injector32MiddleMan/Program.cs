using System.Text.Json;
using InjectorCommon;
using InjectorCommon.Core;

namespace Injector32MiddleMan;

internal class Program
{
    static int Main(string[] args)
    {
        if(!TryParseArguments(args, out int processId, out string dllPath, out InjectionResult earlyExitResult))
        {
            PrintResultAndExit(earlyExitResult, 1);
            return 1;
        }

        InjectionResult injectionResult = PerformInjection(processId, dllPath);
        return PrintResultAndExit(injectionResult, injectionResult.Success ? 0 : 1);


        static bool TryParseArguments(string[] args, out int processId, out string dllPath, out InjectionResult errorResult)
        {
            processId = 0;
            dllPath = null;
            errorResult = null;

            if(args.Length < 2)
            {
                errorResult = new InjectionResult { Success = false, Message = "Usage: Injector32MiddleMan.exe <PID> <DLL_PATH>", ModuleHandle = 0L };
                return false;
            }

            if(!int.TryParse(args[0], out processId))
            {
                errorResult = new InjectionResult { Success = false, Message = "Invalid PID provided.", ModuleHandle = 0L };
                return false;
            }

            dllPath = args[1];

            return true;
        }

        static InjectionResult PerformInjection(int processId, string dllPath) => Injector.Inject(processId, dllPath);

        static int PrintResultAndExit(InjectionResult result, int exitCode)
        {
            try
            {
                string jsonResult = JsonSerializer.Serialize(result);
                Console.WriteLine(jsonResult);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"Critical error: Failed to serialize injection result: {ex.Message}");
                var fallbackResult = new InjectionResult { Success = false, Message = $"Internal serialization error: {ex.Message}", ModuleHandle = 0L };
                Console.WriteLine(JsonSerializer.Serialize(fallbackResult));
                return 1;
            }
            return exitCode;
        }
    }
}