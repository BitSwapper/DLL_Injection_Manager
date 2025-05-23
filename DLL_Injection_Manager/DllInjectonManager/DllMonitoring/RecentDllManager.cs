using System.Text.Json;

namespace DLL_Injection_Manager.DllInjectonManager.DllMonitoring;

public class RecentDllManager
{
    List<string> recentDllPaths = new List<string>();
    readonly ListBox listBoxRecentDlls;
    readonly int maxRecentDlls;

    public RecentDllManager(ListBox listBox, int maxItems)
    {
        listBoxRecentDlls = listBox ?? throw new ArgumentNullException(nameof(listBox));
        maxRecentDlls = maxItems > 0 ? maxItems : 50;
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
                    recentDllPaths.AddRange(loadedPaths);
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
        object previouslySelected = listBoxRecentDlls.SelectedItem;
        listBoxRecentDlls.Items.Clear();

        foreach(string path in recentDllPaths)
            if(File.Exists(path))
                listBoxRecentDlls.Items.Add(new DllListItem(path));

        if(previouslySelected != null && listBoxRecentDlls.Items.Contains(previouslySelected))
            listBoxRecentDlls.SelectedItem = previouslySelected;

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
