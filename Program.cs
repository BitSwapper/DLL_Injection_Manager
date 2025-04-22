using DLL_Injection_Manager.DllInjectonManager;

namespace DLL_Injection_Manager;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormDllInjector());
    }
}