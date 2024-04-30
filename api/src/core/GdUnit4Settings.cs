namespace GdUnit4.Core;

using Godot;

/// <summary>
/// Gets access to the GdUnit4 settings to be configured in the Godot editor.
/// </summary>
internal sealed class GdUnit4Settings
{
    internal const string MAIN_CATEGORY = "gdunit4";
    // Common Settings
    internal const string COMMON_SETTINGS = MAIN_CATEGORY + "/settings";

    internal const string GROUP_COMMON = COMMON_SETTINGS + "/common";
    internal const string UPDATE_NOTIFICATION_ENABLED = GROUP_COMMON + "/update_notification_enabled";
    internal const string SERVER_TIMEOUT = GROUP_COMMON + "/server_connection_timeout_minutes";

    internal const string GROUP_TEST = COMMON_SETTINGS + "/test";
    internal const string TEST_TIMEOUT = GROUP_TEST + "/test_timeout_seconds";
    internal const string TEST_LOOKUP_FOLDER = GROUP_TEST + "/test_lookup_folder";
    //internal const string TEST_SITE_NAMING_CONVENTION = GROUP_TEST + "/test_suite_naming_convention";


    // Report Settings
    internal const string REPORT_SETTINGS = MAIN_CATEGORY + "/report";
    internal const string GROUP_GODOT = REPORT_SETTINGS + "/godot";
    internal const string REPORT_PUSH_ERRORS = GROUP_GODOT + "/push_error";
    internal const string REPORT_SCRIPT_ERRORS = GROUP_GODOT + "/script_error";
    internal const string REPORT_ORPHANS = REPORT_SETTINGS + "/verbose_orphans";
    internal const string GROUP_ASSERT = REPORT_SETTINGS + "/assert";
    internal const string REPORT_ASSERT_WARNINGS = GROUP_ASSERT + "/verbose_warnings";
    internal const string REPORT_ASSERT_ERRORS = GROUP_ASSERT + "/verbose_errors";
    internal const string REPORT_ASSERT_STRICT_NUMBER_TYPE_COMPARE = GROUP_ASSERT + "/strict_number_type_compare";

    // Godot debug stdout/logging settings
    internal const string CATEGORY_LOGGING = "debug/file_logging/";
    internal const string STDOUT_ENABLE_TO_FILE = CATEGORY_LOGGING + "enable_file_logging";
    internal const string STDOUT_WITE_TO_FILE = CATEGORY_LOGGING + "log_path";


    // GdUnit Templates
    //internal const string TEMPLATES = MAIN_CATEGORY + "/templates";
    //internal const string TEMPLATES_TS = TEMPLATES + "/testsuite";
    //internal const string TEMPLATE_TS_GD = TEMPLATES_TS + "/GDScript";
    //internal const string TEMPLATE_TS_CS = TEMPLATES_TS + "/CSharpScript";

    // UI Settings
    internal const string UI_SETTINGS = MAIN_CATEGORY + "/ui";
    internal const string GROUP_UI_INSPECTOR = UI_SETTINGS + "/inspector";
    internal const string INSPECTOR_NODE_COLLAPSE = GROUP_UI_INSPECTOR + "/node_collapse";


    // Shortcut Settings
    /**
    internal const string SHORTCUT_SETTINGS = MAIN_CATEGORY + "/Shortcuts";
    internal const string GROUP_SHORTCUT_INSPECTOR = SHORTCUT_SETTINGS + "/inspector";
    internal const string SHORTCUT_INSPECTOR_RERUN_TEST = GROUP_SHORTCUT_INSPECTOR + "/rerun_test";
    internal const string SHORTCUT_INSPECTOR_RERUN_TEST_DEBUG = GROUP_SHORTCUT_INSPECTOR + "/rerun_test_debug";
    internal const string SHORTCUT_INSPECTOR_RUN_TEST_OVERALL = GROUP_SHORTCUT_INSPECTOR + "/run_test_overall";
    internal const string SHORTCUT_INSPECTOR_RUN_TEST_STOP = GROUP_SHORTCUT_INSPECTOR + "/run_test_stop";

    internal const string GROUP_SHORTCUT_EDITOR = SHORTCUT_SETTINGS + "/editor";
    internal const string SHORTCUT_EDITOR_RUN_TEST = GROUP_SHORTCUT_EDITOR + "/run_test";
    internal const string SHORTCUT_EDITOR_RUN_TEST_DEBUG = GROUP_SHORTCUT_EDITOR + "/run_test_debug";
    internal const string SHORTCUT_EDITOR_CREATE_TEST = GROUP_SHORTCUT_EDITOR + "/create_test";

    internal const string GROUP_SHORTCUT_FILESYSTEM = SHORTCUT_SETTINGS + "/filesystem";
    internal const string SHORTCUT_FILESYSTEM_RUN_TEST = GROUP_SHORTCUT_FILESYSTEM + "/run_test";
    internal const string SHORTCUT_FILESYSTEM_RUN_TEST_DEBUG = GROUP_SHORTCUT_FILESYSTEM + "/run_test_debug";
    */

    // Toolbar Settings
    internal const string GROUP_UI_TOOLBAR = UI_SETTINGS + "/toolbar";
    internal const string INSPECTOR_TOOLBAR_BUTTON_RUN_OVERALL = GROUP_UI_TOOLBAR + "/run_overall";

    // defaults
    // server connection timeout in minutes
    internal const int DEFAULT_SERVER_TIMEOUT = 30;
    // test case runtime timeout in seconds
    internal const int DEFAULT_TEST_TIMEOUT = 60 * 5;
    // the folder to create new test-suites
    internal const string DEFAULT_TEST_LOOKUP_FOLDER = "test";


    public static T? GetSetting<T>(string name, T @default)
        => ProjectSettings.HasSetting(name)
         ? ProjectSettings.GetSetting(name).UnboxVariant()
         : @default;

    public static bool IsUpdateNotificationEnabled()
        => ProjectSettings.HasSetting(UPDATE_NOTIFICATION_ENABLED) && (bool)ProjectSettings.GetSetting(UPDATE_NOTIFICATION_ENABLED);

    public static void SetUpdateNotification(bool enable)
    {
        ProjectSettings.SetSetting(UPDATE_NOTIFICATION_ENABLED, enable);
        ProjectSettings.Save();
    }

    public static string GetLogPath()
        => (string)ProjectSettings.GetSetting(STDOUT_WITE_TO_FILE);

    public static void SetLogPath(string path)
    {
        ProjectSettings.SetSetting(STDOUT_ENABLE_TO_FILE, true);
        ProjectSettings.SetSetting(STDOUT_WITE_TO_FILE, path);
        ProjectSettings.Save();
    }

    // the configured server connection timeout in ms
    public static int ServerTimeout()
        => GetSetting(SERVER_TIMEOUT, DEFAULT_SERVER_TIMEOUT) * 60 * 1000;

    // the configured test case timeout in ms
    public static int TestTimeout()
        => GetSetting(TEST_TIMEOUT, DEFAULT_TEST_TIMEOUT) * 1000;

    // the root folder to store/generate test-suites
    public static string? TestRootFolder()
        => GetSetting(TEST_LOOKUP_FOLDER, DEFAULT_TEST_LOOKUP_FOLDER);

    public static bool IsVerboseAssertWarnings()
        => GetSetting(REPORT_ASSERT_WARNINGS, true);

    public static bool IsVerboseAssertErrors()
        => GetSetting(REPORT_ASSERT_ERRORS, true);

    public static bool IsVerboseOrphans()
        => GetSetting(REPORT_ORPHANS, true);

    public static bool IsStrictNumberTypeCompare()
        => GetSetting(REPORT_ASSERT_STRICT_NUMBER_TYPE_COMPARE, true);

    public static bool IsReportPushErrors()
        => GetSetting(REPORT_PUSH_ERRORS, false);

    public static bool IsReportScriptErrors()
        => GetSetting(REPORT_SCRIPT_ERRORS, true);

    public static bool IsInspectorNodeCollapse()
        => GetSetting(INSPECTOR_NODE_COLLAPSE, true);

    public static bool IsInspectorToolbarButtonShow()
        => GetSetting(INSPECTOR_TOOLBAR_BUTTON_RUN_OVERALL, true);

    public static bool IsLogEnabled()
        => (bool)ProjectSettings.GetSetting(STDOUT_ENABLE_TO_FILE);
}
