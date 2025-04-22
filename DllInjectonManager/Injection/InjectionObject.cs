using AddyTools.DllInjectonManager.Constants;
using AddyTools.DllInjectonManager.DllMonitoring;
using AddyTools.DllInjectonManager.ProcessMonitoring;
using AddyTools.DllInjectonManager.UI;

namespace AddyTools.DllInjectonManager.Injection;

public class InjectionObject
{
    readonly ProcessMonitor processMonitor;
    readonly RecentDllManager recentDllManager;
    readonly UIManager uiManager;
    readonly Func<ProcessItem> refreshProcessListAction;
    readonly Action saveSettingsAction;
    readonly Control ownerControl;

    public InjectionObject(ProcessMonitor procMonitor, RecentDllManager recentDllMgr, UIManager uiMgr, Func<ProcessItem> refreshProcList, Action saveCfg, Control owner)
    {
        processMonitor = procMonitor ?? throw new ArgumentNullException(nameof(procMonitor));
        recentDllManager = recentDllMgr ?? throw new ArgumentNullException(nameof(recentDllMgr));
        uiManager = uiMgr ?? throw new ArgumentNullException(nameof(uiMgr));
        refreshProcessListAction = refreshProcList ?? throw new ArgumentNullException(nameof(refreshProcList));
        saveSettingsAction = saveCfg ?? throw new ArgumentNullException(nameof(saveCfg));
        ownerControl = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public void PerformInjection(ProcessItem selectedProcessItem, DllListItem selectedDllItem)
    {
        string targetProcessName = selectedProcessItem?.Name;
        if(string.IsNullOrEmpty(targetProcessName))
        {
            uiManager.ShowWarning(StringConstants.MsgProcessRequired, StringConstants.TitleProcessRequired);
            return;
        }

        if(selectedDllItem == null)
        {
            uiManager.ShowWarning(StringConstants.MsgDllRequired, StringConstants.TitleDllRequired);
            return;
        }

        string currentDllPath = selectedDllItem.FullPath;
        if(string.IsNullOrEmpty(currentDllPath))
        {
            uiManager.ShowWarning(StringConstants.MsgDllRequired, StringConstants.TitleDllRequired);
            return;
        }

        if(!File.Exists(currentDllPath))
        {
            uiManager.ShowWarning(string.Format(StringConstants.MsgDllNotFoundFormat, Environment.NewLine, currentDllPath), StringConstants.TitleDllNotFound);
            recentDllManager.Remove(currentDllPath);
            saveSettingsAction();
            return;
        }

        string fullDllPath = Path.GetFullPath(currentDllPath);
        string dllFileName = Path.GetFileName(fullDllPath);

        uiManager.SetStatus($"Refreshing process list to find '{targetProcessName}'...", ColorConstants.StatusColorDefault);
        ownerControl?.Update();
        ProcessItem targetProcess = refreshProcessListAction();

        if(targetProcess == null)
        {
            uiManager.SetStatus($"Process '{targetProcessName}' not found after refresh.", ColorConstants.StatusColorError);
            uiManager.ShowError(string.Format(StringConstants.MsgProcessNotFoundFormat, targetProcessName), StringConstants.TitleProcessNotFound);
            uiManager.UpdateButtonStates();
            return;
        }

        ownerControl.Invoke((MethodInvoker)delegate { uiManager.UpdateButtonStates(); });

        try
        {
            int processId = targetProcess.Id;

            if(processMonitor.IsDllLoaded(processId, fullDllPath))
            {
                DialogResult result2 = uiManager.ShowConfirmation(string.Format(StringConstants.MsgDllPossiblyLoadedFormat, dllFileName, targetProcess.ToString(), Environment.NewLine), StringConstants.TitleDllPossiblyLoaded);
                if(result2 == DialogResult.No)
                {
                    uiManager.SetStatus(StringConstants.StatusInjectionCancelled, ColorConstants.StatusColorDefault);
                    uiManager.UpdateUIDueToSelectionChange();
                    return;
                }
            }

            uiManager.SetStatus(string.Format(StringConstants.StatusInjectingFormat, dllFileName, targetProcess.ToString(), processId), ColorConstants.StatusColorWarning);
            ownerControl?.Update();

            var injector = new DllInjector();
            var result = injector.InjectDll(processId, fullDllPath);

            if(result.Success)
            {
                processMonitor.TrackLoadedDll(processId, fullDllPath, result.ModuleHandle);
                uiManager.SetDllLoadState(true);
                uiManager.SetStatus(StringConstants.StatusInjectionSuccess, ColorConstants.StatusColorSuccess);
                recentDllManager.AddOrUpdate(fullDllPath);
                saveSettingsAction();
            }
            else
            {
                uiManager.SetDllLoadState(false);
                uiManager.ShowError(string.Format(StringConstants.StatusInjectionFailedFormat, result.Message), StringConstants.TitleInjectionFailed);
                uiManager.SetStatus(string.Format(StringConstants.StatusInjectionFailedFormat, result.Message), ColorConstants.StatusColorError);
            }
        }
        catch(Exception ex)
        {
            uiManager.SetDllLoadState(false);
            uiManager.ShowError(string.Format(StringConstants.StatusErrorInjectionFormat, ex.Message, Environment.NewLine, ex.StackTrace), StringConstants.TitleInjectionError);
            uiManager.SetStatus(StringConstants.StatusErrorGeneric, ColorConstants.StatusColorError);
        }
        finally
        {
            uiManager.UpdateUIDueToSelectionChange();
        }
    }
}