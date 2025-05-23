using System.Diagnostics;
using System.Text.Json;
using FormDllInjector.DllMonitoring;
using FormDllInjector.ProcessMonitoring;
using FormDllInjector.Statics;
using FormDllInjector.UI;
using InjectorCommon;
using ProcessUtils = FormDllInjector.Utils.ProcessUtils;

namespace FormDllInjector.Injection;

public class InjectionOrchestrator
{
    readonly ProcessMonitor _processMonitor;
    readonly RecentDllManager _recentDllManager;
    readonly UIManager _uiManager;
    readonly Func<ProcessItem> _refreshProcessListAction;
    readonly Action _saveSettingsAction;
    readonly Control _ownerControl;

    public InjectionOrchestrator(ProcessMonitor procMonitor, RecentDllManager recentDllMgr, UIManager uiMgr, Func<ProcessItem> refreshProcList, Action saveCfg, Control owner)
    {
        _processMonitor = procMonitor;
        _recentDllManager = recentDllMgr;
        _uiManager = uiMgr;
        _refreshProcessListAction = refreshProcList;
        _saveSettingsAction = saveCfg;
        _ownerControl = owner;
    }

    public async Task PerformInjectionAsync(ProcessItem selectedProcessItem, DllListItem selectedDllItem)
    {
        if(!ValidateInputs(selectedProcessItem, selectedDllItem, out string validatedDllPath))
            return;

        if(!RefreshAndValidateTargetProcess(selectedProcessItem, out ProcessItem targetProcess))
            return;

        if(IsDllAlreadyLoadedAndUserCancels(targetProcess.Id, validatedDllPath, selectedDllItem.FileName, targetProcess.ToString()))
            return;

        UpdateUIForInjectionStart(selectedDllItem.FileName, targetProcess.ToString(), targetProcess.Id);

        InjectionResult injectionResult;
        try
        {
            injectionResult = await ExecuteInjectionLogicAsync(targetProcess, validatedDllPath);
            ProcessInjectionOutcome(injectionResult, targetProcess.Id, validatedDllPath);
        }
        catch(Exception ex)
        {
            HandleGenericInjectionException(ex, targetProcess.Id, validatedDllPath);
        }
        finally
        {
            _uiManager.UpdateUIDueToSelectionChange();
        }
    }

    bool ValidateInputs(ProcessItem selectedProcessItem, DllListItem selectedDllItem, out string validatedDllPath)
    {
        validatedDllPath = null;
        if(selectedProcessItem == null)
        {
            _uiManager.ShowWarning(StaticStringsFormDllInjector.MsgProcessRequired, StaticStringsFormDllInjector.TitleProcessRequired);
            return false;
        }

        if(selectedDllItem == null || string.IsNullOrEmpty(selectedDllItem.FullPath))
        {
            _uiManager.ShowWarning(StaticStringsFormDllInjector.MsgDllRequired, StaticStringsFormDllInjector.TitleDllRequired);
            return false;
        }

        validatedDllPath = Path.GetFullPath(selectedDllItem.FullPath);
        if(!File.Exists(validatedDllPath))
        {
            _uiManager.ShowWarning(string.Format(StaticStringsFormDllInjector.MsgDllNotFoundFormat, Environment.NewLine, validatedDllPath), StaticStringsFormDllInjector.TitleDllNotFound);
            _recentDllManager.Remove(validatedDllPath);
            _saveSettingsAction();
            _uiManager.UpdateUIDueToSelectionChange();
            return false;
        }
        return true;
    }

    bool RefreshAndValidateTargetProcess(ProcessItem initialSelectedProcess, out ProcessItem validatedTargetProcess)
    {
        _uiManager.SetStatus($"Refreshing process list for '{initialSelectedProcess.Name}'...", StaticColors.StatusDefault);
        _ownerControl?.Update();

        validatedTargetProcess = _refreshProcessListAction();
        if(validatedTargetProcess == null || validatedTargetProcess.Id != initialSelectedProcess.Id)
        {
            _uiManager.SetStatus($"Process '{initialSelectedProcess.Name}' not found or changed. Please re-select.", StaticColors.StatusError);
            _uiManager.ShowError(string.Format(StaticStringsFormDllInjector.MsgProcessNotFoundFormat, initialSelectedProcess.Name), StaticStringsFormDllInjector.TitleProcessNotFound);
            _uiManager.UpdateUIDueToSelectionChange();
            return false;
        }
        return true;
    }

    bool IsDllAlreadyLoadedAndUserCancels(int targetProcessId, string dllPath, string dllFileName, string targetProcessString)
    {
        if(_processMonitor.IsDllLoaded(targetProcessId, dllPath))
        {
            DialogResult dResult = _uiManager.ShowConfirmation(
                string.Format(StaticStringsFormDllInjector.MsgDllPossiblyLoadedFormat, dllFileName, targetProcessString, Environment.NewLine),
                StaticStringsFormDllInjector.TitleDllPossiblyLoaded);
            if(dResult == DialogResult.No)
            {
                _uiManager.SetStatus(StaticStringsFormDllInjector.StatusInjectionCancelled, StaticColors.StatusDefault);
                _uiManager.UpdateUIDueToSelectionChange();
                return true;
            }
        }
        return false;
    }

    void UpdateUIForInjectionStart(string dllFileName, string targetProcessString, int targetProcessId)
    {
        _uiManager.SetStatus(string.Format(StaticStringsFormDllInjector.StatusInjectingFormat, dllFileName, targetProcessString, targetProcessId), StaticColors.StatusWarning);
        _ownerControl?.Update();
    }

    async Task<InjectionResult> ExecuteInjectionLogicAsync(ProcessItem targetProcess, string dllPath)
    {
        InjectorCommon.Utils.ProcessBitness targetBitness = ProcessUtils.GetTargetProcessBitness(targetProcess.Id);
        string injectorExeName = GetInjectorExeNameForBitness(targetBitness);

        if(string.IsNullOrEmpty(injectorExeName))
            return new InjectionResult { Success = false, Message = "Could not determine target process bitness or no supported injector found.", ModuleHandle = 0L };

        return await InjectWithHelperAsync(injectorExeName, targetProcess.Id, dllPath);
    }

    string GetInjectorExeNameForBitness(InjectorCommon.Utils.ProcessBitness bitness)
    {
        switch(bitness)
        {
            case InjectorCommon.Utils.ProcessBitness.Bit64:
                return "Injector64MiddleMan.exe";
            case InjectorCommon.Utils.ProcessBitness.Bit32:
                return "Injector32MiddleMan.exe";
            default:
                return null;
        }
    }

    void ProcessInjectionOutcome(InjectionResult injectionResult, int targetProcessId, string dllPath)
    {
        if(injectionResult.Success)
        {
            _processMonitor.TrackLoadedDll(targetProcessId, dllPath, (nint)injectionResult.ModuleHandle);
            _uiManager.SetDllLoadState(true);
            _uiManager.SetStatus(StaticStringsFormDllInjector.StatusInjectionSuccess, StaticColors.StatusSuccess);
            _recentDllManager.AddOrUpdate(dllPath);
            _saveSettingsAction();
        }
        else
        {
            _processMonitor.UntrackDll(targetProcessId, dllPath);
            _uiManager.SetDllLoadState(false);
            _uiManager.ShowError(string.Format(StaticStringsFormDllInjector.StatusInjectionFailedFormat, injectionResult.Message), StaticStringsFormDllInjector.TitleInjectionFailed);
            _uiManager.SetStatus(string.Format(StaticStringsFormDllInjector.StatusInjectionFailedFormat, injectionResult.Message), StaticColors.StatusError);
        }
    }

    void HandleGenericInjectionException(Exception ex, int targetProcessId, string dllPath)
    {
        _processMonitor.UntrackDll(targetProcessId, dllPath);
        _uiManager.SetDllLoadState(false);
        _uiManager.ShowError(string.Format(StaticStringsFormDllInjector.StatusErrorInjectionFormat, ex.Message, Environment.NewLine, ex.StackTrace), StaticStringsFormDllInjector.TitleInjectionError);
        _uiManager.SetStatus(string.Format(StaticStringsFormDllInjector.StatusErrorGenericFormat, ex.Message), StaticColors.StatusError);
    }

    async Task<InjectionResult> InjectWithHelperAsync(string injectorExeName, int processId, string dllPath)
    {
        string helperPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, injectorExeName);
        if(!File.Exists(helperPath))
            return new InjectionResult { Success = false, Message = $"{injectorExeName} not found at '{helperPath}'.", ModuleHandle = 0L };

        ProcessStartInfo startInfo = new ProcessStartInfo(helperPath)
        {
            Arguments = $"{processId} \"{dllPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
        };

        try
        {
            using Process helperProcess = new Process { StartInfo = startInfo };
            helperProcess.Start();

            Task<string> readOutputTask = helperProcess.StandardOutput.ReadToEndAsync();
            Task<string> readErrorTask = helperProcess.StandardError.ReadToEndAsync();

            bool processExited = await Task.WhenAny(Task.Run(() => helperProcess.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds)), Task.Delay(Timeout.Infinite)) == Task.Run(() => helperProcess.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds));

            string output = await readOutputTask;
            string error = await readErrorTask;

            LogHelperProcessOutput(injectorExeName, output, error);

            if(!processExited && !helperProcess.HasExited)
            {
                try { helperProcess.Kill(true); } catch { }
                return new InjectionResult { Success = false, Message = $"{injectorExeName} timed out.", ModuleHandle = 0L };
            }


            if(!string.IsNullOrWhiteSpace(output))
                return DeserializeHelperOutput(output, injectorExeName, error);
            else
            {
                string errorMessage = $"{injectorExeName} produced no standard output. Exit Code: {helperProcess.ExitCode}.";
                if(!string.IsNullOrWhiteSpace(error))
                    errorMessage += $" Stderr: {error}";
                return new InjectionResult { Success = false, Message = errorMessage, ModuleHandle = 0L };
            }
        }
        catch(Exception ex)
        {
            return new InjectionResult { Success = false, Message = $"Exception launching/communicating with {injectorExeName}: {ex.Message}", ModuleHandle = 0L };
        }
    }

    void LogHelperProcessOutput(string injectorExeName, string output, string error)
    {
        System.Diagnostics.Debug.WriteLine($"--- {injectorExeName} STDOUT START ---");
        System.Diagnostics.Debug.WriteLine(output);
        System.Diagnostics.Debug.WriteLine($"--- {injectorExeName} STDOUT END ---");
        if(!string.IsNullOrWhiteSpace(error))
        {
            System.Diagnostics.Debug.WriteLine($"--- {injectorExeName} STDERR START ---");
            System.Diagnostics.Debug.WriteLine(error);
            System.Diagnostics.Debug.WriteLine($"--- {injectorExeName} STDERR END ---");
        }
    }

    InjectionResult DeserializeHelperOutput(string output, string injectorExeName, string errorStreamContent)
    {
        try
        {
            var result = JsonSerializer.Deserialize<InjectionResult>(output);
            if(result == null)
                return new InjectionResult { Success = false, Message = $"Failed to parse result from {injectorExeName} (deserialized to null). Output: {output}", ModuleHandle = 0L };
            return result;
        }
        catch(JsonException jsonEx)
        {
            string errorMessage = $"Error parsing {injectorExeName} JSON output: {jsonEx.Message}. Output: {output}";
            if(!string.IsNullOrWhiteSpace(errorStreamContent))
                errorMessage += $". Stderr: {errorStreamContent}";
            return new InjectionResult { Success = false, Message = errorMessage, ModuleHandle = 0L };
        }
    }
}