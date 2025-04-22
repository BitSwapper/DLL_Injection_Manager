using AddyTools.DllInjectonManager.Constants;
using AddyTools.DllInjectonManager.DllMonitoring;
using AddyTools.DllInjectonManager.ProcessMonitoring;

namespace AddyTools.DllInjectonManager.UI;

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
        Color currentStatusColor = ColorConstants.StatusColorDefault;

        if(selectedProcess != null && selectedDll != null)
        {
            string currentDllPath = selectedDll.FullPath;
            if(!string.IsNullOrEmpty(currentDllPath))
            {
                if(File.Exists(currentDllPath))
                {
                    isDllCurrentlyLoaded = processMonitor.IsDllLoaded(selectedProcess.Id, currentDllPath);
                    currentStatusMessage = isDllCurrentlyLoaded
                        ? string.Format(StringConstants.StatusDllAlreadyLoadedFormat, selectedDll.FileName, selectedProcess.ToString())
                        : string.Format(StringConstants.StatusReadyToInjectFormat, selectedDll.FileName, selectedProcess.ToString());
                    currentStatusColor = isDllCurrentlyLoaded ? ColorConstants.StatusColorWarning : ColorConstants.StatusColorDefault;
                }
                else
                {
                    currentStatusMessage = string.Format(StringConstants.StatusDllFileNotFoundFormat, selectedDll.FileName);
                    currentStatusColor = ColorConstants.StatusColorError;
                }
            }
        }

        UpdateButtonStates();

        bool isCurrentlyInjecting = previousColor == ColorConstants.StatusColorWarning && previousText.StartsWith("Injecting");

        if(!isCurrentlyInjecting)
        {
            if(string.IsNullOrEmpty(currentStatusMessage) && previousColor == ColorConstants.StatusColorUnloaded && previousText.Contains("unloaded"))
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