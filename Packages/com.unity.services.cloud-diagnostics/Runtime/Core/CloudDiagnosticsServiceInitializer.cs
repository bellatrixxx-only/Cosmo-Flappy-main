using UnityEngine;
using Unity.Services.Core.Device.Internal;
using Unity.Services.Core.Internal;
using Task = System.Threading.Tasks.Task;

namespace Unity.Services.CloudDiagnostics
{
    class CloudDiagnosticsInitializer : IInitializablePackage
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Register()
        {
            CoreRegistry.Instance.RegisterPackage(new CloudDiagnosticsInitializer())
                .DependsOn<IInstallationId>();
        }

        public Task Initialize(CoreRegistry registry)
        {
            IInstallationId installationId = registry.GetServiceComponent<IInstallationId>();
            SetNativeInstallationIdentifier(installationId.GetOrCreateIdentifier());

            return Task.CompletedTask;
        }

        static void SetNativeInstallationIdentifier(string installId)
        {
#if (UNITY_2022_2_OR_NEWER && ENABLE_CLOUD_SERVICES_CRASH_REPORTING)
            UnityEngine.CrashReportHandler.CrashReportHandler.installationIdentifier = installId;
#endif
        }
    }
}
