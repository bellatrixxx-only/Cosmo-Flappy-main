using System.Collections.Generic;
using Unity.Services.Core.Editor;
using UnityEditor;
using UnityEditor.CrashReporting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.CloudDiagnostics.Editor
{
    class CloudDiagnosticsSettingsProvider : EditorGameServiceSettingsProvider
    {
        static readonly string[] k_SupportedPlatforms = new string[] { "Android", "iOS", "Linux", "Mac", "PC", "WebGL", "Windows 8 Universal", "Windows 10 Universal" };
        const int k_CrashBufferLogMinimum = 0;
        const int k_CrashBufferLogMaximum = 50;

        public CloudDiagnosticsSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords) {}

        protected override IEditorGameService EditorGameService => EditorGameServiceRegistry.Instance.GetEditorGameService<CloudDiagnosticsIdentifier>();
        protected override string Title => "Cloud Diagnostics";
        protected override string Description => "Discover app errors.";

        protected override VisualElement GenerateServiceDetailUI()
        {
            var containerAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath.Common);
            VisualElement containerUI = null;
            if (containerAsset != null)
            {
                containerUI = containerAsset.CloneTree().contentContainer;
                SetupStyleSheets(containerUI);

                AddCrashReportingUI(containerUI);
                SetupLearnMoreButton(containerUI);
            }

            return containerUI;
        }

        void SetupStyleSheets(VisualElement parentElement)
        {
            parentElement.AddToClassList("clouddiag");

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath.Common);
            if (styleSheet != null)
            {
                parentElement.styleSheets.Add(styleSheet);
            }
        }

        void AddCrashReportingUI(VisualElement parentElement)
        {
            var crashReportContainer = parentElement.Q(className: UssClassName.CrashReportContainer);
            var crashReportCommonAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath.CrashReportCommon);
            if (crashReportContainer != null && crashReportCommonAsset != null)
            {
                var crashReportCommonVisual = crashReportCommonAsset.CloneTree().contentContainer;
                crashReportContainer.Clear();
                crashReportContainer.Add(crashReportCommonVisual);
                crashReportContainer.Add(PlatformSupportUiHelper.GeneratePlatformSupport(k_SupportedPlatforms));

                SetupGoToDashboard(crashReportContainer);

                if (EditorGameService.Enabler.IsEnabled())
                {
                    AddEnabledCrashReportUI(crashReportContainer);
                }
            }
        }

        void SetupGoToDashboard(VisualElement parentVisualElement)
        {
            var crashServiceGoToDashboard = parentVisualElement.Q(UxmlNode.CrashReportGoToDashboard);
            if (crashServiceGoToDashboard != null)
            {
                var clickable = new Clickable(() =>
                {
                    EditorGameService.OpenDashboard();
                });
                crashServiceGoToDashboard.AddManipulator(clickable);
            }
        }

        void AddEnabledCrashReportUI(VisualElement parentElement)
        {
            var crashReportEnableContainer = parentElement.Q(UxmlNode.CrashReportEnabledContainer);
            var crashReportEnableAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath.CrashReportEnabled);
            if (crashReportEnableContainer != null && crashReportEnableAsset != null)
            {
                var crashReportEnableVisual = crashReportEnableAsset.CloneTree().contentContainer;
                crashReportEnableContainer.Clear();
                crashReportEnableContainer.Add(crashReportEnableVisual);

                SetupCapturePlayModeToggle(crashReportEnableContainer);
                SetupLogBufferSize(crashReportEnableContainer);
            }
        }

        static void SetupCapturePlayModeToggle(VisualElement parentElement)
        {
            var capturePlayModeToggle = parentElement.Q<Toggle>(UxmlNode.CrashReportCapturePlayMode);
            if (capturePlayModeToggle != null)
            {
                capturePlayModeToggle.SetValueWithoutNotify(CrashReportingSettings.captureEditorExceptions);
                capturePlayModeToggle.RegisterValueChangedCallback(evt =>
                {
                    EditorGameServiceAnalyticsSender.SendProjectSettingsCapturePlaymodeEvent(evt.newValue);
                    CrashReportingSettings.captureEditorExceptions = evt.newValue;
                });
            }
        }

        static void SetupLogBufferSize(VisualElement parentElement)
        {
            var logBufferSize = parentElement.Q<IntegerField>(UxmlNode.CrashReportBufferLogCount);
            if (logBufferSize != null)
            {
                logBufferSize.SetValueWithoutNotify((int)CrashReportingSettings.logBufferSize);
                logBufferSize.RegisterValueChangedCallback(evt =>
                {
                    var newValue = evt.newValue;
                    var updateValue = false;

                    if (evt.newValue < k_CrashBufferLogMinimum)
                    {
                        newValue = k_CrashBufferLogMinimum;
                        updateValue = true;
                    }
                    else if (evt.newValue > k_CrashBufferLogMaximum)
                    {
                        newValue = k_CrashBufferLogMaximum;
                        updateValue = true;
                    }

                    CrashReportingSettings.logBufferSize = (uint)newValue;
                    if (updateValue)
                    {
                        logBufferSize.SetValueWithoutNotify(newValue);
                    }
                });
            }
        }

        static void SetupLearnMoreButton(VisualElement parentElement)
        {
            var learnMoreButton = parentElement.Q(UxmlNode.LearnMore);
            if (learnMoreButton != null)
            {
                var clickable = new Clickable(() =>
                {
                    EditorGameServiceAnalyticsSender.SendProjectSettingsLearnMoreEvent();
                    Application.OpenURL(URL.LearnMore);
                });
                learnMoreButton.AddManipulator(clickable);
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
#if ENABLE_EDITOR_GAME_SERVICES
            return new CloudDiagnosticsSettingsProvider(GenerateProjectSettingsPath("Cloud Diagnostics"), SettingsScope.Project);
#else
            return null;
#endif
        }

        static class URL
        {
            public const string LearnMore = "https://docs.unity.com/cloud-diagnostics/GettingStarted/GettingStartedwithCloudDiagnostics.html";
        }

        static class UxmlPath
        {
            public const string Common = "Packages/com.unity.services.cloud-diagnostics/Editor/UXML/CloudDiagProjectSettings.uxml";
            public const string CrashReportCommon = "Packages/com.unity.services.cloud-diagnostics/Editor/UXML/CloudDiagProjectSettingsCrash.uxml";
            public const string CrashReportEnabled = "Packages/com.unity.services.cloud-diagnostics/Editor/UXML/CloudDiagProjectSettingsCrashEnabled.uxml";
        }

        static class UxmlNode
        {
            public const string CrashReportGoToDashboard = "GoToDashboard";
            public const string CrashReportEnabledContainer = "CloudDiagCrashStateContainer";
            public const string CrashReportCapturePlayMode = "CrashCapturePlayMode";
            public const string CrashReportBufferLogCount = "CrashLogCount";
            public const string LearnMore = "LearnMore";
        }

        static class UssClassName
        {
            public const string CrashReportContainer = "cloud-diag-crash";
        }

        static class UssPath
        {
            public const string Common = "Packages/com.unity.services.cloud-diagnostics/Editor/USS/CloudDiagStyleSheet.uss";
        }
    }
}
