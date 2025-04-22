namespace AddyTools.DllInjectonManager.DllMonitoring;

public class DllListItem
{
    public string FullPath { get; }
    public string FileName => Path.GetFileName(FullPath);

    public DllListItem(string fullPath) => FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));

    public override bool Equals(object obj) => obj is DllListItem other && FullPath.Equals(other.FullPath, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => FullPath.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => FileName;
}