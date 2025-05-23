using System.Text.Json;

namespace FormDllInjector.DllMonitoring;

public class RecentDllManager
{
    List<string> recentDllPaths = new List<string>();
    readonly ListBox listBoxRecentDlls;
    readonly int maxRecentDlls;

    public RecentDllManager(ListBox listBox, int maxItems)
    {
        listBoxRecentDlls = listBox ?? throw new ArgumentNullException(nameof(listBox));
        maxRecentDlls = maxItems > 0 ? maxItems : 15;
        listBoxRecentDlls.DisplayMember = nameof(DllListItem.FileName);
    }

    public DllListItem SelectedDll => listBoxRecentDlls.SelectedItem as DllListItem;

    public string SerializePaths() => JsonSerializer.Serialize(recentDllPaths);

    public void LoadPaths(string serializedPaths)
    {
        recentDllPaths.Clear();
        if(!string.IsNullOrEmpty(serializedPaths))
        {
            try
            {
                var loadedPaths = JsonSerializer.Deserialize<List<string>>(serializedPaths);
                if(loadedPaths != null)
                    recentDllPaths.AddRange(loadedPaths.Where(p => !string.IsNullOrWhiteSpace(p)));
            }
            catch(JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to deserialize recent DLL paths: {ex.Message}");
            }
        }
        UpdateListBox();
        SelectFirstItem();
    }

    public void AddOrUpdate(string dllPath)
    {
        if(string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath)) return;

        string fullPath = Path.GetFullPath(dllPath);
        recentDllPaths.RemoveAll(p => p.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
        recentDllPaths.Insert(0, fullPath);

        TrimList();
        UpdateListBox();
        SelectItemByPath(fullPath);
    }

    public void Remove(string dllPath)
    {
        if(string.IsNullOrWhiteSpace(dllPath)) return;
        string fullPath = Path.GetFullPath(dllPath);
        int removedCount = recentDllPaths.RemoveAll(p => p.Equals(fullPath, StringComparison.OrdinalIgnoreCase));

        if(removedCount > 0)
        {
            UpdateListBox();
            SelectFirstItem();
        }
    }

    void UpdateListBox()
    {
        listBoxRecentDlls.BeginUpdate();
        object previouslySelectedItem = listBoxRecentDlls.SelectedItem;
        listBoxRecentDlls.Items.Clear();

        foreach(string path in recentDllPaths)
        {
            if(File.Exists(path)) // Ensure file exists before adding
                listBoxRecentDlls.Items.Add(new DllListItem(path));
        }


        if(previouslySelectedItem != null && listBoxRecentDlls.Items.Cast<DllListItem>().Any(item => item.FullPath == (previouslySelectedItem as DllListItem)?.FullPath))
            listBoxRecentDlls.SelectedItem = listBoxRecentDlls.Items.Cast<DllListItem>().FirstOrDefault(item => item.FullPath == (previouslySelectedItem as DllListItem)?.FullPath);
        else
            SelectFirstItem();


        listBoxRecentDlls.EndUpdate();
    }


    void TrimList()
    {
        if(recentDllPaths.Count > maxRecentDlls)
            recentDllPaths.RemoveRange(maxRecentDlls, recentDllPaths.Count - maxRecentDlls);
    }

    void SelectItemByPath(string fullPath)
    {
        var itemToSelect = listBoxRecentDlls.Items.OfType<DllListItem>()
                                .FirstOrDefault(item => item.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
        if(itemToSelect != null)
            listBoxRecentDlls.SelectedItem = itemToSelect;
    }

    void SelectFirstItem()
    {
        if(listBoxRecentDlls.Items.Count > 0 && listBoxRecentDlls.SelectedIndex == -1)
            listBoxRecentDlls.SelectedIndex = 0;
        else if(listBoxRecentDlls.Items.Count == 0)
            listBoxRecentDlls.SelectedItem = null;
    }
}