namespace DLL_Injection_Manager.DllInjectonManager.DllMonitoring;

public class DllListItem
{
    public string FullPath { get; }
    public string FileName => Path.GetFileName(FullPath);

    public DllListItem(string fullPath) => FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
}