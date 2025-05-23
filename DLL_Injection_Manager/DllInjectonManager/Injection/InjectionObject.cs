using DLL_Injection_Manager.DllInjectonManager.Constants;
using DLL_Injection_Manager.DllInjectonManager.DllMonitoring;
using DLL_Injection_Manager.DllInjectonManager.ProcessMonitoring;
using DLL_Injection_Manager.DllInjectonManager.UI;

namespace DLL_Injection_Manager.DllInjectonManager.Injection;

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
            uiManager.ShowWarning(Constants_Strings_FormDllInjector.MsgProcessRequired, Constants_Strings_FormDllInjector.TitleProcessRequired);
            return;
        }

        if(selectedDllItem == null)
        {
            uiManager.ShowWarning(Constants_Strings_FormDllInjector.MsgDllRequired, Constants_Strings_FormDllInjector.TitleDllRequired);
            return;
        }

        string currentDllPath = selectedDllItem.FullPath;
        if(string.IsNullOrEmpty(currentDllPath))
        {
            uiManager.ShowWarning(Constants_Strings_FormDllInjector.MsgDllRequired, Constants_Strings_FormDllInjector.TitleDllRequired);
            return;
        }

        if(!File.Exists(currentDllPath))
        {
            uiManager.ShowWarning(string.Format(Constants_Strings_FormDllInjector.MsgDllNotFoundFormat, Environment.NewLine, currentDllPath), Constants_Strings_FormDllInjector.TitleDllNotFound);
            recentDllManager.Remove(currentDllPath);
            saveSettingsAction();
            return;
        }

        string fullDllPath = Path.GetFullPath(currentDllPath);
        string dllFileName = Path.GetFileName(fullDllPath);

        uiManager.SetStatus($"Refreshing process list to find '{targetProcessName}'...", Constants_Colors.StatusDefault);
        ownerControl?.Update();
        ProcessItem targetProcess = refreshProcessListAction();

        if(targetProcess == null)
        {
            uiManager.SetStatus($"Process '{targetProcessName}' not found after refresh.", Constants_Colors.StatusError);
            uiManager.ShowError(string.Format(Constants_Strings_FormDllInjector.MsgProcessNotFoundFormat, targetProcessName), Constants_Strings_FormDllInjector.TitleProcessNotFound);
            uiManager.UpdateButtonStates();
            return;
        }

        ownerControl.Invoke((MethodInvoker)delegate { uiManager.UpdateButtonStates(); });

        try
        {
            int processId = targetProcess.Id;

            if(processMonitor.IsDllLoaded(processId, fullDllPath))
            {
                DialogResult dResult = uiManager.ShowConfirmation(string.Format(Constants_Strings_FormDllInjector.MsgDllPossiblyLoadedFormat, dllFileName, targetProcess.ToString(), Environment.NewLine), Constants_Strings_FormDllInjector.TitleDllPossiblyLoaded);
                if(dResult == DialogResult.No)
                {
                    uiManager.SetStatus(Constants_Strings_FormDllInjector.StatusInjectionCancelled, Constants_Colors.StatusDefault);
                    uiManager.UpdateUIDueToSelectionChange();
                    return;
                }
            }

            uiManager.SetStatus(string.Format(Constants_Strings_FormDllInjector.StatusInjectingFormat, dllFileName, targetProcess.ToString(), processId), Constants_Colors.StatusWarning);
            ownerControl?.Update();

            var injector = new DllInjector();
            var result = injector.InjectDll(processId, fullDllPath);

            if(result.Success)
            {
                processMonitor.TrackLoadedDll(processId, fullDllPath, result.ModuleHandle);
                uiManager.SetDllLoadState(true);
                uiManager.SetStatus(Constants_Strings_FormDllInjector.StatusInjectionSuccess, Constants_Colors.StatusSuccess);
                recentDllManager.AddOrUpdate(fullDllPath);
                saveSettingsAction();
            }
            else
            {
                uiManager.SetDllLoadState(false);
                uiManager.ShowError(string.Format(Constants_Strings_FormDllInjector.StatusInjectionFailedFormat, result.Message), Constants_Strings_FormDllInjector.TitleInjectionFailed);
                uiManager.SetStatus(string.Format(Constants_Strings_FormDllInjector.StatusInjectionFailedFormat, result.Message), Constants_Colors.StatusError);
            }
        }
        catch(Exception ex)
        {
            uiManager.SetDllLoadState(false);
            uiManager.ShowError(string.Format(Constants_Strings_FormDllInjector.StatusErrorInjectionFormat, ex.Message, Environment.NewLine, ex.StackTrace), Constants_Strings_FormDllInjector.TitleInjectionError);
            uiManager.SetStatus(Constants_Strings_FormDllInjector.StatusErrorGeneric, Constants_Colors.StatusError);
        }
        finally
        {
            uiManager.UpdateUIDueToSelectionChange();
        }
    }
}