namespace AddyTools.DllInjectonManager.ProcessMonitoring;

public class ProcessItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string DisplayName => $"{Id}: {Name}";

    public ProcessItem(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => DisplayName;
}

