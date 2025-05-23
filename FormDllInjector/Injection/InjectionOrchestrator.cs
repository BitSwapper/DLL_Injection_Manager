using System.Diagnostics;
using System.Text.Json;
using FormDllInjector.DllMonitoring;
using FormDllInjector.ProcessMonitoring;
using FormDllInjector.Statics;
using FormDllInjector.UI;
using FormDllInjector.Utils;
using InjectorCommon; // Use the shared InjectionResult

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
        if(selectedProcessItem == null)
        {
            _uiManager.ShowWarning(StaticStringsFormDllInjector.MsgProcessRequired, StaticStringsFormDllInjector.TitleProcessRequired);
            return;
        }
        if(selectedDllItem == null || string.IsNullOrEmpty(selectedDllItem.FullPath))
        {
            _uiManager.ShowWarning(StaticStringsFormDllInjector.MsgDllRequired, StaticStringsFormDllInjector.TitleDllRequired);
            return;
        }

        string dllPath = Path.GetFullPath(selectedDllItem.FullPath);
        if(!File.Exists(dllPath))
        {
            _uiManager.ShowWarning(string.Format(StaticStringsFormDllInjector.MsgDllNotFoundFormat, Environment.NewLine, dllPath), StaticStringsFormDllInjector.TitleDllNotFound);
            _recentDllManager.Remove(dllPath);
            _saveSettingsAction();
            _uiManager.UpdateUIDueToSelectionChange();
            return;
        }

        _uiManager.SetStatus($"Refreshing process list for '{selectedProcessItem.Name}'...", StaticColors.StatusDefault);
        _ownerControl?.Update();

        ProcessItem targetProcess = _refreshProcessListAction();
        if(targetProcess == null || targetProcess.Id != selectedProcessItem.Id)
        {
            _uiManager.SetStatus($"Process '{selectedProcessItem.Name}' not found or changed. Please re-select.", StaticColors.StatusError);
            _uiManager.ShowError(string.Format(StaticStringsFormDllInjector.MsgProcessNotFoundFormat, selectedProcessItem.Name), StaticStringsFormDllInjector.TitleProcessNotFound);
            _uiManager.UpdateUIDueToSelectionChange();
            return;
        }

        if(_processMonitor.IsDllLoaded(targetProcess.Id, dllPath))
        {
            DialogResult dResult = _uiManager.ShowConfirmation(
                string.Format(StaticStringsFormDllInjector.MsgDllPossiblyLoadedFormat, selectedDllItem.FileName, targetProcess.ToString(), Environment.NewLine),
                StaticStringsFormDllInjector.TitleDllPossiblyLoaded);
            if(dResult == DialogResult.No)
            {
                _uiManager.SetStatus(StaticStringsFormDllInjector.StatusInjectionCancelled, StaticColors.StatusDefault);
                _uiManager.UpdateUIDueToSelectionChange();
                return;
            }
        }

        _uiManager.SetStatus(string.Format(StaticStringsFormDllInjector.StatusInjectingFormat, selectedDllItem.FileName, targetProcess.ToString(), targetProcess.Id), StaticColors.StatusWarning);
        _ownerControl?.Update();

        InjectionResult injectionResult = null;
        ProcessUtils.ProcessBitness bitness = ProcessUtils.GetTargetProcessBitness(targetProcess.Id);

        try
        {
            string injectorExeName = "";
            if(bitness == ProcessUtils.ProcessBitness.Bit64)
            {
                injectorExeName = "Injector64.exe";
            }
            else if(bitness == ProcessUtils.ProcessBitness.Bit32)
            {
                injectorExeName = "Injector32.exe";
            }
            else
            {
                injectionResult = new InjectionResult { Success = false, Message = "Could not determine target process bitness or unsupported.", ModuleHandle = 0L };
            }

            if(!string.IsNullOrEmpty(injectorExeName))
            {
                injectionResult = await InjectWithHelperAsync(injectorExeName, targetProcess.Id, dllPath);
            }


            if(injectionResult.Success)
            {
                _processMonitor.TrackLoadedDll(targetProcess.Id, dllPath, (nint)injectionResult.ModuleHandle); // Cast long to nint
                _uiManager.SetDllLoadState(true);
                _uiManager.SetStatus(StaticStringsFormDllInjector.StatusInjectionSuccess, StaticColors.StatusSuccess);
                _recentDllManager.AddOrUpdate(dllPath);
                _saveSettingsAction();
            }
            else
            {
                _processMonitor.UntrackDll(targetProcess.Id, dllPath);
                _uiManager.SetDllLoadState(false);
                _uiManager.ShowError(string.Format(StaticStringsFormDllInjector.StatusInjectionFailedFormat, injectionResult.Message), StaticStringsFormDllInjector.TitleInjectionFailed);
                _uiManager.SetStatus(string.Format(StaticStringsFormDllInjector.StatusInjectionFailedFormat, injectionResult.Message), StaticColors.StatusError);
            }
        }
        catch(Exception ex)
        {
            _processMonitor.UntrackDll(targetProcess.Id, dllPath);
            _uiManager.SetDllLoadState(false);
            _uiManager.ShowError(string.Format(StaticStringsFormDllInjector.StatusErrorInjectionFormat, ex.Message, Environment.NewLine, ex.StackTrace), StaticStringsFormDllInjector.TitleInjectionError);
            _uiManager.SetStatus(string.Format(StaticStringsFormDllInjector.StatusErrorGenericFormat, ex.Message), StaticColors.StatusError);
        }
        finally
        {
            _uiManager.UpdateUIDueToSelectionChange();
        }
    }

    async Task<InjectionResult> InjectWithHelperAsync(string injectorExeName, int processId, string dllPath)
    {
        string helperPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, injectorExeName);
        if(!File.Exists(helperPath))
        {
            return new InjectionResult { Success = false, Message = $"{injectorExeName} not found.", ModuleHandle = 0L };
        }

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

            string output = await helperProcess.StandardOutput.ReadToEndAsync();
            string error = await helperProcess.StandardError.ReadToEndAsync(); // Capture stderr separately

            // For debugging: Log the raw output
            System.Diagnostics.Debug.WriteLine($"--- {injectorExeName} STDOUT START ---");
            System.Diagnostics.Debug.WriteLine(output);
            System.Diagnostics.Debug.WriteLine($"--- {injectorExeName} STDOUT END ---");
            if(!string.IsNullOrWhiteSpace(error))
            {
                System.Diagnostics.Debug.WriteLine($"--- {injectorExeName} STDERR START ---");
                System.Diagnostics.Debug.WriteLine(error);
                System.Diagnostics.Debug.WriteLine($"--- {injectorExeName} STDERR END ---");
            }

            await helperProcess.WaitForExitAsync(new CancellationTokenSource(10000).Token);

            if(helperProcess.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                try
                {
                    var result = JsonSerializer.Deserialize<InjectionResult>(output);
                    return result ?? new InjectionResult { Success = false, Message = $"Failed to parse result from {injectorExeName} (null result). Output: {output}", ModuleHandle = 0L };
                }
                catch(JsonException jsonEx)
                {
                    return new InjectionResult { Success = false, Message = $"Error parsing {injectorExeName} JSON output: {jsonEx.Message}. Output: {output}", ModuleHandle = 0L };
                }
            }
            else
            {
                string errorMessage = $"{injectorExeName} failed or produced no output. Exit Code: {helperProcess.ExitCode}.";
                if(!string.IsNullOrWhiteSpace(error)) errorMessage += $" Stderr: {error}";
                if(string.IsNullOrWhiteSpace(output) && helperProcess.ExitCode == 0) errorMessage += " Helper process exited successfully but produced no standard output.";
                else if(!string.IsNullOrWhiteSpace(output)) errorMessage += $" Stdout: {output}"; // Include stdout if it exists but exit code was non-zero

                return new InjectionResult { Success = false, Message = errorMessage, ModuleHandle = 0L };
            }
        }
        catch(TaskCanceledException)
        {
            return new InjectionResult { Success = false, Message = $"{injectorExeName} timed out.", ModuleHandle = 0L };
        }
        catch(Exception ex)
        {
            return new InjectionResult { Success = false, Message = $"Exception launching/communicating with {injectorExeName}: {ex.Message}", ModuleHandle = 0L };
        }
    }
}