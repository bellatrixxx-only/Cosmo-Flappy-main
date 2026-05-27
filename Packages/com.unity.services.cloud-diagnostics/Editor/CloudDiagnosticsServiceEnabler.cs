using Unity.Services.Core.Editor;
using UnityEditor.CrashReporting;
using UnityEditor.PackageManager;

namespace Unity.Services.CloudDiagnostics.Editor
{
    class CloudDiagnosticsEditorGameServiceEnabler : EditorGameServiceFlagEnabler
    {
        const string CloudDiagnosticsPackageName = "com.unity.services.cloud-diagnostics";
        protected override string FlagName { get; } = "gameperf";

        protected override void EnableLocalSettings()
        {
            CrashReportingSettings.enabled = true;
            Events.registeringPackages += RegisteringPackagesEventHandler;
        }

        protected override void DisableLocalSettings()
        {
            CrashReportingSettings.enabled = false;
            Events.registeringPackages -= RegisteringPackagesEventHandler;
        }

        public override bool IsEnabled()
        {
            return CrashReportingSettings.enabled;
        }

        void RegisteringPackagesEventHandler(PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            bool isModification = false;
            foreach (PackageInfo package in packageRegistrationEventArgs.changedTo)
            {
                if (package.name.Equals(CloudDiagnosticsPackageName))
                {
                    isModification = true;
                    break;
                }
            }
            // Only disable CrashReporting if the package is being removed and not being updated.
            if (!isModification)
            {
                foreach (PackageInfo package in packageRegistrationEventArgs.removed)
                {
                    if (package.name.Equals(CloudDiagnosticsPackageName))
                    {
                        CrashReportingSettings.enabled = false;
                        break;
                    }
                }
            }
        }
    }
}
