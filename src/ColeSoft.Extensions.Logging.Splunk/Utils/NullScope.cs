using System;

namespace ColeSoft.Extensions.Logging.Splunk.Utils
{
    /// <summary>
    /// An empty scope without any logic.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Just a stub.")]
    public class NullScope : IDisposable
    {
        private NullScope()
        {
        }

        /// <summary>
        /// The null instance.
        /// </summary>
        public static NullScope Instance { get; } = new NullScope();

        /// <inheritdoc />
        public void Dispose()
        {
            // Method intentionally left empty.
        }
    }
}
