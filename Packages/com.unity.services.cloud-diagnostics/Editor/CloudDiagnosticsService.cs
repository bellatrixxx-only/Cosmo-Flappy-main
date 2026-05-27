using Unity.Services.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.CloudDiagnostics.Editor
{
    class CloudDiagnosticsService : IEditorGameService
    {
        public string Name => "Cloud Diagnostics";
        public IEditorGameServiceIdentifier Identifier { get; } = new CloudDiagnosticsIdentifier();
        public bool RequiresCoppaCompliance => false;
        public bool HasDashboard => true;
        public string GetFormattedDashboardUrl()
        {
#if ENABLE_EDITOR_GAME_SERVICES
            return $"https://developer.cloud.unity3d.com/diagnostics/orgs/{CloudProjectSettings.organizationKey}/projects/{CloudProjectSettings.projectId}/crashes";
#else
            return string.Empty;
#endif
        }

        public IEditorGameServiceEnabler Enabler { get; } = new CloudDiagnosticsEditorGameServiceEnabler();
    }
}
