using FormDllInjector.DllMonitoring;
using FormDllInjector.ProcessMonitoring;
using FormDllInjector.Statics;

namespace FormDllInjector.UI;

public class UIManager
{
    readonly Label lblStatus;
    readonly Button btnInject;
    readonly ListBox listBoxProcesses;
    readonly RecentDllManager recentDllManager;
    readonly ProcessMonitor processMonitor;
    bool isDllCurrentlyLoadedInternal;

    public UIManager(Label statusLabel, Button injectButton, ListBox processListBox, RecentDllManager recentDllMgr, ProcessMonitor procMonitor)
    {
        lblStatus = statusLabel;
        btnInject = injectButton;
        listBoxProcesses = processListBox;
        recentDllManager = recentDllMgr;
        processMonitor = procMonitor;
    }

    public bool IsDllCurrentlyLoaded => isDllCurrentlyLoadedInternal;

    public void SetDllLoadState(bool loaded) => isDllCurrentlyLoadedInternal = loaded;

    public void UpdateUIDueToSelectionChange()
    {
        ProcessItem selectedProcess = listBoxProcesses.SelectedItem as ProcessItem;
        DllListItem selectedDll = recentDllManager.SelectedDll;

        string currentStatusMessage = "Select a process and DLL to inject.";
        Color currentStatusColor = StaticColors.StatusDefault;
        bool enableInjectButton = false;

        if(selectedProcess != null && selectedDll != null)
        {
            string currentDllPath = selectedDll.FullPath;
            if(!string.IsNullOrEmpty(currentDllPath))
            {
                if(File.Exists(currentDllPath))
                {
                    isDllCurrentlyLoadedInternal = processMonitor.IsDllLoaded(selectedProcess.Id, currentDllPath);
                    if(isDllCurrentlyLoadedInternal)
                    {
                        currentStatusMessage = string.Format(StaticStringsFormDllInjector.StatusDllAlreadyLoadedFormat, selectedDll.FileName, selectedProcess.ToString());
                        currentStatusColor = StaticColors.StatusWarning;
                        enableInjectButton = true;
                    }
                    else
                    {
                        currentStatusMessage = string.Format(StaticStringsFormDllInjector.StatusReadyToInjectFormat, selectedDll.FileName, selectedProcess.ToString());
                        currentStatusColor = StaticColors.StatusDefault;
                        enableInjectButton = true;
                    }
                }
                else
                {
                    currentStatusMessage = string.Format(StaticStringsFormDllInjector.StatusDllFileNotFoundFormat, selectedDll.FileName);
                    currentStatusColor = StaticColors.StatusError;
                    enableInjectButton = false;
                }
            }
            else
            {
                currentStatusMessage = "Selected DLL path is invalid.";
                currentStatusColor = StaticColors.StatusError;
                enableInjectButton = false;
            }
        }
        else if(selectedProcess == null && selectedDll != null)
            currentStatusMessage = "Select a target process.";
        else if(selectedProcess != null && selectedDll == null)
            currentStatusMessage = "Select a DLL to inject.";

        bool isCurrentlyInjecting = lblStatus.Text.StartsWith("Injecting") && lblStatus.ForeColor == StaticColors.StatusWarning;
        if(!isCurrentlyInjecting)
            SetStatus(currentStatusMessage, currentStatusColor);

        if(btnInject.InvokeRequired)
            btnInject.Invoke((MethodInvoker)delegate { btnInject.Enabled = enableInjectButton; });
        else
            btnInject.Enabled = enableInjectButton;
    }


    public void SetStatus(string text, Color color)
    {
        if(lblStatus.Text == text && lblStatus.ForeColor == color) return;

        if(lblStatus.InvokeRequired)
            lblStatus.Invoke((MethodInvoker)delegate { lblStatus.Text = text; lblStatus.ForeColor = color; });
        else
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }
    }

    public void ShowError(string message, string title) => MessageBox.Show(_ownerControl, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    public void ShowWarning(string message, string title) => MessageBox.Show(_ownerControl, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public DialogResult ShowConfirmation(string message, string title) => MessageBox.Show(_ownerControl, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    Control _ownerControl => listBoxProcesses.TopLevelControl as Form;

}