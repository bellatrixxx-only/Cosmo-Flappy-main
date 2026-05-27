using UnityEditor;

namespace Unity.Services.CloudDiagnostics.Editor
{
    static class CloudDiagnosticsTopMenu
    {
        const int k_ConfigureMenuPriority = 100;
        const string k_ServiceMenuRoot = "Services/Cloud Diagnostics/";

        [MenuItem(k_ServiceMenuRoot + "Configure", priority = k_ConfigureMenuPriority)]
        static void ShowProjectSettings()
        {
            EditorGameServiceAnalyticsSender.SendTopMenuConfigureEvent();
            SettingsService.OpenProjectSettings("Project/Services/Cloud Diagnostics");
        }
    }
}
