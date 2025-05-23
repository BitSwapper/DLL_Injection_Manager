using System.Text.Json;
using Injector32.Core;
using InjectorCommon;

namespace Injector32;

internal class Program
{
    static int Main(string[] args)
    {
        InjectionResult result;

        if(args.Length < 2)
        {
            result = new InjectionResult { Success = false, Message = "Usage: Injector32.exe <PID> <DLL_PATH>", ModuleHandle = 0L };
            Console.WriteLine(JsonSerializer.Serialize(result));
            return 1;
        }

        if(!int.TryParse(args[0], out int processId))
        {
            result = new InjectionResult { Success = false, Message = "Invalid PID.", ModuleHandle = 0L };
            Console.WriteLine(JsonSerializer.Serialize(result));
            return 1;
        }

        string dllPath = args[1];
        if(!File.Exists(dllPath))
        {
            result = new InjectionResult { Success = false, Message = $"DLL not found: {dllPath}", ModuleHandle = 0L };
            Console.WriteLine(JsonSerializer.Serialize(result));
            return 1;
        }

        result = InjectorCore.InjectDllInternal(processId, dllPath);

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

        return result.Success ? 0 : 1;
    }
}