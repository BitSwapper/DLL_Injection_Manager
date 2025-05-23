using FormDllInjector.Properties;

namespace FormDllInjector;

public class SettingsManager
{
    public void LoadSettings(Action<string, string> applySettingsCallback)
    {
        try
        {
            string lastDll = Settings.Default.LastDllPath;
            string lastProcess = Settings.Default.LastProcessName;
            applySettingsCallback(lastProcess, lastDll);
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
    }

    public void SaveSettings(string processName, string dllPath)
    {
        try
        {
            Settings.Default.LastProcessName = processName;
            Settings.Default.LastDllPath = dllPath;
            Settings.Default.Save();
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
}