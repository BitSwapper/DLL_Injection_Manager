namespace FormDllInjector.ProcessMonitoring;

public class ProcessItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string DisplayName => $"{Name} ({Id})";

    public ProcessItem(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => DisplayName;
}