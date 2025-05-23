using System.Diagnostics;

namespace FormDllInjector.ProcessMonitoring;

public static class ProcessListProvider
{
    public static List<ProcessItem> GetAllProcesses()
    {
        var processes = new List<ProcessItem>();
        try
        {
            Process[] systemProcesses = Process.GetProcesses();
            Array.Sort(systemProcesses, (p1, p2) => string.Compare(p1.ProcessName, p2.ProcessName, StringComparison.OrdinalIgnoreCase));

            foreach(Process process in systemProcesses)
            {
                try
                {
                    if(!string.IsNullOrEmpty(process.ProcessName) && process.MainWindowHandle != IntPtr.Zero) // Basic filter for GUI apps
                    {
                        processes.Add(new ProcessItem(process.Id, process.ProcessName));
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Skipping process ID {process.Id}. Error accessing info: {ex.Message}");
                }
                finally
                {
                    process?.Dispose();
                }
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error getting process list: {ex.Message}");
            throw;
        }
        return processes;
    }
}