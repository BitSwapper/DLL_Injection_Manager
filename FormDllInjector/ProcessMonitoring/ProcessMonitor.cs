using System.Text;
using FormDllInjector.Native;

namespace FormDllInjector.ProcessMonitoring;

public class ProcessMonitor
{
    readonly Dictionary<int, Dictionary<string, nint>> loadedModules;
    readonly INativeMethodsProvider nativeMethods;

    public ProcessMonitor(INativeMethodsProvider methodsProvider)
    {
        loadedModules = new Dictionary<int, Dictionary<string, nint>>();
        nativeMethods = methodsProvider ?? throw new ArgumentNullException(nameof(methodsProvider));
    }

    public void TrackProcess(int processId)
    {
        if(!loadedModules.ContainsKey(processId))
            loadedModules[processId] = new Dictionary<string, nint>(StringComparer.OrdinalIgnoreCase);
    }

    public void TrackLoadedDll(int processId, string dllPath, nint moduleHandle)
    {
        TrackProcess(processId);
        loadedModules[processId][dllPath.ToLowerInvariant()] = moduleHandle;
    }

    public void UntrackDll(int processId, string dllPath)
    {
        if(loadedModules.TryGetValue(processId, out var processDlls))
            processDlls.Remove(dllPath.ToLowerInvariant());
    }

    public bool IsDllLoaded(int processId, string dllPath)
    {
        if(string.IsNullOrEmpty(dllPath)) return false;

        string lowerDllPath = dllPath.ToLowerInvariant();

        if(loadedModules.TryGetValue(processId, out var processDlls) && processDlls.ContainsKey(lowerDllPath))
        {
            if(IsDllActuallyLoadedInProcess(processId, dllPath))
                return true;

            processDlls.Remove(lowerDllPath);
            return false;
        }

        bool isLoaded = IsDllActuallyLoadedInProcess(processId, dllPath);
        if(isLoaded)
            TrackLoadedDll(processId, dllPath, nint.Zero);

        return isLoaded;
    }

    public void CleanupTerminatedProcesses()
    {
        List<int> processesToRemove = new List<int>();
        foreach(int processId in loadedModules.Keys)
        {
            try
            {
                using var process = System.Diagnostics.Process.GetProcessById(processId);
                if(process.HasExited)
                    processesToRemove.Add(processId);
            }
            catch
            {
                processesToRemove.Add(processId);
            }
        }

        foreach(int processId in processesToRemove)
            loadedModules.Remove(processId);
    }

    bool IsDllActuallyLoadedInProcess(int processId, string dllPath)
    {
        nint processHandle = nint.Zero;
        bool isLoaded = false;
        const uint access = InjectorCommon.Native.NativeMethods.PROCESS_QUERY_INFORMATION | InjectorCommon.Native.NativeMethods.PROCESS_VM_READ;

        try
        {
            processHandle = nativeMethods.OpenProcess(access, false, processId);
            if(processHandle == nint.Zero) return false;

            string targetDllName = Path.GetFileName(dllPath);
            if(string.IsNullOrEmpty(targetDllName)) return false;

            nint[] moduleHandles = new nint[1024];
            uint arraySize = (uint)(nint.Size * moduleHandles.Length);

            if(nativeMethods.EnumProcessModulesEx(processHandle, moduleHandles, arraySize, out uint bytesNeeded, NativeMethods_Form.LIST_MODULES_ALL))
            {
                int moduleCount = (int)(bytesNeeded / nint.Size);
                StringBuilder moduleNameBuilder = new StringBuilder((int)NativeMethods_Form.MAX_PATH);

                for(int i = 0; i < moduleCount; i++)
                {
                    moduleNameBuilder.Clear();
                    if(nativeMethods.GetModuleFileNameExW(processHandle, moduleHandles[i], moduleNameBuilder, (uint)NativeMethods_Form.MAX_PATH) > 0)
                    {
                        string currentModulePath = moduleNameBuilder.ToString();
                        if(dllPath.Equals(currentModulePath, StringComparison.OrdinalIgnoreCase))
                        {
                            isLoaded = true;
                            break;
                        }
                    }
                }
            }
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking loaded modules for PID {processId}: {ex.Message}");
            return false;
        }
        finally
        {
            if(processHandle != nint.Zero)
                nativeMethods.CloseHandle(processHandle);
        }
        return isLoaded;
    }
}