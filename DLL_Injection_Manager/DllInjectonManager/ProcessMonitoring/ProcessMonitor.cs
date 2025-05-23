using System.Text;

namespace DLL_Injection_Manager.DllInjectonManager.ProcessMonitoring;

public class ProcessMonitor
{
    Dictionary<int, Dictionary<string, nint>> loadedModules;

    public ProcessMonitor() => loadedModules = new Dictionary<int, Dictionary<string, nint>>();

    public void TrackProcess(int processId)
    {
        if(!loadedModules.ContainsKey(processId))
            loadedModules[processId] = new Dictionary<string, nint>(StringComparer.OrdinalIgnoreCase);
    }

    public void TrackLoadedDll(int processId, string dllPath, nint moduleHandle)
    {
        TrackProcess(processId);
        loadedModules[processId][dllPath.ToLower()] = moduleHandle;
    }

    public bool IsDllLoaded(int processId, string dllPath)
    {
        if(string.IsNullOrEmpty(dllPath))
            return false;

        if(loadedModules.TryGetValue(processId, out var processDlls) && processDlls.ContainsKey(dllPath.ToLower()))
        {
            if(IsDllLoadedInProcess(processId, dllPath))
                return true;
            else
            {
                processDlls.Remove(dllPath.ToLower());
                return false;
            }
        }

        bool isLoaded = IsDllLoadedInProcess(processId, dllPath);
        if(isLoaded)
            TrackLoadedDll(processId, dllPath, nint.Zero);

        return isLoaded;
    }

    public void CleanupTerminatedProcesses()
    {
        List<int> processesToRemove = new List<int>();

        foreach(int processId in loadedModules.Keys)
            try
            {
                nint processHandle = NativeFunctions.OpenProcess(NativeFunctions.PROCESS_QUERY_INFORMATION, false, processId);

                if(processHandle == nint.Zero)
                    processesToRemove.Add(processId);
                else
                    NativeFunctions.CloseHandle(processHandle);
            }
            catch
            {
                processesToRemove.Add(processId);
            }

        foreach(int processId in processesToRemove)
            loadedModules.Remove(processId);
    }

    bool IsDllLoadedInProcess(int processId, string dllPath)
    {
        nint processHandle = nint.Zero;
        bool isLoaded = false;
        const uint access = NativeFunctions.PROCESS_QUERY_INFORMATION | NativeFunctions.PROCESS_VM_READ;

        try
        {
            processHandle = NativeFunctions.OpenProcess(access, false, processId);
            if(processHandle == nint.Zero)
                return false;

            string targetDllName = Path.GetFileName(dllPath);
            if(string.IsNullOrEmpty(targetDllName)) return false;

            uint bytesNeeded;
            nint[] moduleHandles = new nint[1024];
            uint arraySize = (uint)(nint.Size * moduleHandles.Length);

            if(NativeFunctions.EnumProcessModulesEx(processHandle, moduleHandles, arraySize, out bytesNeeded, NativeFunctions.LIST_MODULES_ALL))
            {
                int moduleCount = (int)(bytesNeeded / nint.Size);
                StringBuilder moduleNameBuilder = new StringBuilder((int)NativeFunctions.MAX_PATH);

                for(int i = 0; i < moduleCount; i++)
                {
                    moduleNameBuilder.Clear();
                    if(NativeFunctions.GetModuleFileNameExA(processHandle, moduleHandles[i], moduleNameBuilder, NativeFunctions.MAX_PATH) > 0)
                    {
                        string currentModuleName = Path.GetFileName(moduleNameBuilder.ToString());
                        if(targetDllName.Equals(currentModuleName, StringComparison.OrdinalIgnoreCase))
                        {
                            isLoaded = true;
                            break;
                        }
                    }
                }
            }
            else return false;
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking loaded modules for PID {processId}: {ex.Message}");
            return false;
        }
        finally
        {
            if(processHandle != nint.Zero)
                NativeFunctions.CloseHandle(processHandle);
        }

        return isLoaded;
    }
}
