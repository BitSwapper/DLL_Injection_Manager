namespace FormDllInjector.DllMonitoring;

public class DllListItem
{
    public string FullPath { get; }
    public string FileName => Path.GetFileName(FullPath);

    public DllListItem(string fullPath) => FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));

    public override string ToString() => FileName;
}