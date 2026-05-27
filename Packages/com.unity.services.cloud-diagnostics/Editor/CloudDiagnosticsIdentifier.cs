using Unity.Services.Core.Editor;

namespace Unity.Services.CloudDiagnostics.Editor
{
    /// <summary>
    /// Implementation of the <see cref="IEditorGameServiceIdentifier"/> for the Cloud Diagnostics package
    /// </summary>
    public struct CloudDiagnosticsIdentifier : IEditorGameServiceIdentifier
    {
        /// <summary>
        /// Gets the key for the Cloud Diagnostics package
        /// </summary>
        /// <returns>The key for the service</returns>
        public string GetKey() => "CloudDiagnostics";
    }
}
