using DLL_Injection_Manager.DllInjectonManager.Constants;
using DLL_Injection_Manager.DllInjectonManager.DllMonitoring;
using DLL_Injection_Manager.DllInjectonManager.ProcessMonitoring;

namespace DLL_Injection_Manager.DllInjectonManager.UI;

public class UIManager
{
    readonly Label lblStatus;
    readonly Button btnInject;
    readonly ListBox listBoxProcesses;
    readonly RecentDllManager recentDllManager;
    readonly ProcessMonitor processMonitor;
    bool isDllCurrentlyLoaded;

    public UIManager(Label statusLabel, Button injectButton, ListBox processListBox, RecentDllManager recentDllMgr, ProcessMonitor procMonitor)
    {
        lblStatus = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
        btnInject = injectButton ?? throw new ArgumentNullException(nameof(injectButton));
        listBoxProcesses = processListBox ?? throw new ArgumentNullException(nameof(processListBox));
        recentDllManager = recentDllMgr ?? throw new ArgumentNullException(nameof(recentDllMgr));
        processMonitor = procMonitor ?? throw new ArgumentNullException(nameof(procMonitor));
    }

    public void UpdateUIDueToSelectionChange()
    {
        ProcessItem selectedProcess = listBoxProcesses.SelectedItem as ProcessItem;
        DllListItem selectedDll = recentDllManager.SelectedDll;
        Color previousColor = lblStatus.ForeColor;
        string previousText = lblStatus.Text;

        isDllCurrentlyLoaded = false;
        string currentStatusMessage = string.Empty;
        Color currentStatusColor = Constants_Colors.StatusDefault;

        if(selectedProcess != null && selectedDll != null)
        {
            string currentDllPath = selectedDll.FullPath;
            if(!string.IsNullOrEmpty(currentDllPath))
            {
                if(File.Exists(currentDllPath))
                {
                    isDllCurrentlyLoaded = processMonitor.IsDllLoaded(selectedProcess.Id, currentDllPath);

                    currentStatusMessage = isDllCurrentlyLoaded
                        ? string.Format(Constants_Strings_FormDllInjector.StatusDllAlreadyLoadedFormat, selectedDll.FileName, selectedProcess.ToString())
                        : string.Format(Constants_Strings_FormDllInjector.StatusReadyToInjectFormat, selectedDll.FileName, selectedProcess.ToString());

                    currentStatusColor = isDllCurrentlyLoaded ? Constants_Colors.StatusWarning : Constants_Colors.StatusDefault;
                }
                else
                {
                    currentStatusMessage = string.Format(Constants_Strings_FormDllInjector.StatusDllFileNotFoundFormat, selectedDll.FileName);
                    currentStatusColor = Constants_Colors.StatusError;
                }
            }
        }

        UpdateButtonStates();

        bool isCurrentlyInjecting = previousColor == Constants_Colors.StatusWarning && previousText.StartsWith("Injecting");

        if(!isCurrentlyInjecting)
        {
            if(string.IsNullOrEmpty(currentStatusMessage) && previousColor == Constants_Colors.StatusUnloaded && previousText.Contains("unloaded"))
                SetStatus(previousText, previousColor);
            else
                SetStatus(currentStatusMessage, currentStatusColor);
        }
    }

    public void UpdateButtonStates()
    {
        bool dllSelectedAndExists = recentDllManager.SelectedDll != null && !string.IsNullOrEmpty(recentDllManager.SelectedDll.FullPath) && File.Exists(recentDllManager.SelectedDll.FullPath);
        bool canInject = !isDllCurrentlyLoaded && listBoxProcesses.SelectedItem != null && dllSelectedAndExists;

        if(btnInject.InvokeRequired)
            btnInject.Invoke((MethodInvoker)delegate { btnInject.Enabled = canInject; });
        else
            btnInject.Enabled = canInject;
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

    public void ShowError(string message, string title) => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    public void ShowWarning(string message, string title) => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public DialogResult ShowConfirmation(string message, string title) => MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

    public bool IsDllCurrentlyLoaded => isDllCurrentlyLoaded;
    public void SetDllLoadState(bool loaded) => isDllCurrentlyLoaded = loaded;
}