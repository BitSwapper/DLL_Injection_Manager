using System.Diagnostics;
using DLL_Injection_Manager.DllInjectonManager.Constants;
using DLL_Injection_Manager.DllInjectonManager.DllMonitoring;
using DLL_Injection_Manager.DllInjectonManager.Injection;
using DLL_Injection_Manager.DllInjectonManager.ProcessMonitoring;
using DLL_Injection_Manager.DllInjectonManager.UI;
using static DLL_Injection_Manager.DllInjectonManager.Constants.Constants_Strings_FormDllInjector;
using Timer = System.Windows.Forms.Timer;

namespace DLL_Injection_Manager.DllInjectonManager;

public partial class FormDllInjector : Form
{
    const int MonitorTimerIntervalMilliseconds = 200;
    const int MaxRecentDlls = 15;

    List<ProcessItem> allProcesses = new List<ProcessItem>();
    readonly ProcessMonitor processMonitor;
    readonly SettingsManager settingsManager;
    readonly RecentDllManager recentDllManager;
    readonly UIManager uiManager;
    readonly InjectionObject injectionObject;

    public FormDllInjector()
    {
        InitializeComponent();
        processMonitor = new();
        settingsManager = new();
        recentDllManager = new(listBoxRecentDlls, MaxRecentDlls);
        uiManager = new(lblStatus, btnInject, listBoxProcesses, recentDllManager, processMonitor);
        injectionObject = new(processMonitor, recentDllManager, uiManager, RefreshAndSelectProcess, SaveCurrentSettings, this);
    }

    void FormDllInjector_Load(object sender, EventArgs e)
    {
        listBoxProcesses.SelectedIndexChanged += lstProcesses_SelectedIndexChanged;
        listBoxRecentDlls.SelectedIndexChanged += lstRecentDlls_SelectedIndexChanged;
        RefreshProcessListInternal();
        settingsManager.LoadSettings(ApplyLoadedSettings);
        Timer monitorTimer = new() { Interval = MonitorTimerIntervalMilliseconds };
        monitorTimer.Tick += (s, args) => CheckLoadedDllStatus();
        monitorTimer.Start();
    }

    void lstProcesses_SelectedIndexChanged(object sender, EventArgs e) => uiManager.UpdateUIDueToSelectionChange();

    void lstRecentDlls_SelectedIndexChanged(object sender, EventArgs e) => uiManager.UpdateUIDueToSelectionChange();

    void btnRefresh_Click(object sender, EventArgs e)
    {
        txtSearch.Clear();
        RefreshProcessListInternal();
        uiManager.UpdateUIDueToSelectionChange();
    }

    void btnBrowse_Click(object sender, EventArgs e)
    {
        using OpenFileDialog dlg = new ();
        dlg.Filter = $"{DllFilter}|{AllFilesFilter}";
        dlg.Title = SelectDllDialogTitle;
        var selectedDllItem = recentDllManager.SelectedDll;

        if(selectedDllItem != null && File.Exists(selectedDllItem.FullPath))
        {
            string dir = Path.GetDirectoryName(selectedDllItem.FullPath);
            if(Directory.Exists(dir))
                dlg.InitialDirectory = dir;
        }

        if(dlg.ShowDialog() == DialogResult.OK)
        {
            recentDllManager.AddOrUpdate(dlg.FileName);
            SaveCurrentSettings();
            uiManager.UpdateUIDueToSelectionChange();
        }
    }

    void txtSearch_TextChanged(object sender, EventArgs e) => FilterProcessList(txtSearch.Text);

    void btnInject_Click(object sender, EventArgs e) => injectionObject.PerformInjection(listBoxProcesses.SelectedItem as ProcessItem, recentDllManager.SelectedDll);

    void ApplyLoadedSettings(string processName, string serializedDllPaths)
    {
        try
        {
            recentDllManager.LoadPaths(serializedDllPaths);
            if(!string.IsNullOrEmpty(processName))
            {
                txtSearch.Text = processName;
                ProcessItem itemToSelect = listBoxProcesses.Items.OfType<ProcessItem>().FirstOrDefault(p => p.Name.Equals(processName, StringComparison.OrdinalIgnoreCase));
                if(itemToSelect != null)
                    listBoxProcesses.SelectedItem = itemToSelect;
                else if(listBoxProcesses.Items.Count == 1)
                    listBoxProcesses.SelectedIndex = 0;
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine(string.Format(DebugErrorApplyingSettingsFormat, ex.Message));
            uiManager.SetStatus(StatusErrorApplyingSettings, Constants_Colors.StatusError);
            uiManager.ShowWarning(string.Format(DebugErrorApplyingSettingsFormat, ex.Message), TitleSettingsError);
        }
        finally
        {
            uiManager.UpdateUIDueToSelectionChange();
        }
    }

    void SaveCurrentSettings()
    {
        string processName = (listBoxProcesses.SelectedItem as ProcessItem)?.Name ?? string.Empty;
        string serializedDllPaths = recentDllManager.SerializePaths();
        settingsManager.SaveSettings(processName, serializedDllPaths);
    }

    void CheckLoadedDllStatus()
    {
        processMonitor.CleanupTerminatedProcesses();
        var selectedProcess = listBoxProcesses.SelectedItem as ProcessItem;
        var selectedDll = recentDllManager.SelectedDll;
        if(selectedProcess == null || selectedDll == null) return;
        string currentDllPath = selectedDll.FullPath;
        if(string.IsNullOrEmpty(currentDllPath) || !File.Exists(currentDllPath)) return;

        bool isNowLoaded = processMonitor.IsDllLoaded(selectedProcess.Id, currentDllPath);
        bool wasLoaded = uiManager.IsDllCurrentlyLoaded;

        if(wasLoaded != isNowLoaded)
        {
            uiManager.SetDllLoadState(isNowLoaded);
            uiManager.UpdateUIDueToSelectionChange();

            if(!isNowLoaded && lblStatus.ForeColor != Constants_Colors.StatusError)
                uiManager.SetStatus(string.Format(StatusDllUnloadedFormat, selectedProcess.ToString()), Constants_Colors.StatusUnloaded);
        }
    }

    ProcessItem RefreshAndSelectProcess()
    {
        string targetProcessName = (listBoxProcesses.SelectedItem as ProcessItem)?.Name;
        RefreshProcessListInternal();
        if(string.IsNullOrEmpty(targetProcessName)) return null;
        ProcessItem targetProcess = allProcesses.FirstOrDefault(p => p.Name.Equals(targetProcessName, StringComparison.OrdinalIgnoreCase));

        if(targetProcess != null && listBoxProcesses.SelectedItem != targetProcess)
        {
            listBoxProcesses.SelectedItem = targetProcess;
            Application.DoEvents();
        }

        return listBoxProcesses.SelectedItem as ProcessItem;
    }

    void RefreshProcessListInternal()
    {
        string previouslySelectedName = (listBoxProcesses.SelectedItem as ProcessItem)?.Name;
        Cursor = Cursors.WaitCursor;
        listBoxProcesses.BeginUpdate();

        try
        {
            allProcesses = ProcessListProvider.GetAllProcesses();
            FilterProcessList(txtSearch.Text);
            if(!string.IsNullOrEmpty(previouslySelectedName))
            {
                ProcessItem itemToSelect = listBoxProcesses.Items.OfType<ProcessItem>().FirstOrDefault(p => p.Name.Equals(previouslySelectedName, StringComparison.OrdinalIgnoreCase));
                if(itemToSelect != null && listBoxProcesses.SelectedItem != itemToSelect)
                    listBoxProcesses.SelectedItem = itemToSelect;
                else if(itemToSelect == null && listBoxProcesses.SelectedItem != null)
                    listBoxProcesses.ClearSelected();
            }
        }
        catch(Exception ex)
        {
            uiManager.ShowError($"Error refreshing process list: {ex.Message}", TitleGenericError);
            uiManager.SetStatus(StatusErrorLoadingProcesses, Constants_Colors.StatusError);
        }
        finally
        {
            listBoxProcesses.EndUpdate();
            Cursor = Cursors.Default;
        }
    }

    void FilterProcessList(string searchText)
    {
        listBoxProcesses.BeginUpdate();
        object previouslySelectedItem = listBoxProcesses.SelectedItem;
        listBoxProcesses.Items.Clear();
        IEnumerable<ProcessItem> processesToShow = string.IsNullOrWhiteSpace(searchText) ? allProcesses.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase) : allProcesses.Where(p => p.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase);
        listBoxProcesses.Items.AddRange(processesToShow.Cast<object>().ToArray());
        listBoxProcesses.EndUpdate();

        if(previouslySelectedItem != null && listBoxProcesses.Items.Contains(previouslySelectedItem))
            listBoxProcesses.SelectedItem = previouslySelectedItem;
        else if(listBoxProcesses.Items.Count == 1)
            listBoxProcesses.SelectedIndex = 0;
        else
            listBoxProcesses.ClearSelected();

        uiManager.UpdateUIDueToSelectionChange();
    }
}

