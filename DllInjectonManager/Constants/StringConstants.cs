namespace AddyTools.DllInjectonManager.Constants;

public static class StringConstants
{
    public const string DllFilter = "DLL Files (*.dll)|*.dll";
    public const string AllFilesFilter = "All Files (*.*)|*.*";
    public const string SelectDllDialogTitle = "Select DLL File";
    public const string TitleProcessRequired = "Process Required";
    public const string TitleProcessNotFound = "Process Not Found";
    public const string TitleDllRequired = "DLL Required";
    public const string TitleDllNotFound = "DLL Not Found";
    public const string TitleDllPossiblyLoaded = "DLL Possibly Loaded";
    public const string TitleInjectionFailed = "Injection Failed";
    public const string TitleInjectionError = "Injection Error";
    public const string TitleSettingsError = "Settings Error";
    public const string TitleGenericError = "Error";
    public const string MsgProcessRequired = "Please select a process from the list.";
    public const string MsgDllRequired = "Please select a DLL from the list or browse for one.";
    public static readonly string MsgDllNotFoundFormat = "The specified DLL file does not exist:{0}{1}";
    public static readonly string MsgProcessNotFoundFormat = "Could not find a running process named '{0}'. Please refresh the list and select the correct process.";
    public static readonly string MsgDllPossiblyLoadedFormat = "The DLL '{0}' may already be loaded in [{1}].{2}{2}Do you want to attempt to inject it again? (This might be needed if it crashed or unloaded unexpectedly).";
    public static readonly string StatusInjectingFormat = "Injecting '{0}' into [{1} (PID: {2})]...";
    public const string StatusInjectionCancelled = "Injection cancelled by user.";
    public const string StatusInjectionSuccess = "Successfully injected DLL.";
    public static readonly string StatusInjectionFailedFormat = "Injection failed: {0}";
    public static readonly string StatusErrorInjectionFormat = "An unexpected error occurred during injection: {0}{1}{1}{2}";
    public const string StatusErrorGeneric = "Error occurred during injection";
    public static readonly string StatusDllAlreadyLoadedFormat = "DLL '{0}' is already loaded in [{1}].";
    public const string StatusReadyToInjectFormat = "Ready to inject '{0}' into [{1}].";
    public const string StatusDllFileNotFoundFormat = "Selected DLL file not found: {0}";
    public const string StatusErrorApplyingSettings = "Error loading previous settings.";
    public const string StatusErrorLoadingProcesses = "Error loading process list.";
    public static readonly string StatusDllNowLoadedFormat = "DLL is now loaded in [{0}]";
    public static readonly string StatusDllUnloadedFormat = "DLL was unloaded from [{0}]";
    public static readonly string DebugErrorApplyingSettingsFormat = "Error applying settings: {0}";
}