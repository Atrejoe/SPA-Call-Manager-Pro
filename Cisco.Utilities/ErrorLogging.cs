using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RollbarSharp;

namespace Cisco.Utilities
{

    /// <summary>
    /// Simple exception logging, testing multiple exception logging frameworks just for kicks
    /// </summary>
    public static class ErrorLogging
    {

        private static string Environment
        {
            get
            {
#if DEBUG
			return "development";
#else
                return "release";
#endif
            }
        }

        private static readonly Lazy<RollbarClient> RollbarClient = new Lazy<RollbarClient>(GetRollbarClient);
        private static RollbarClient GetRollbarClient()
        {
            var result = new RollbarClient("ca971c37957e4899874dbf864f716501");

            result.Configuration.Environment = Environment;
            result.Configuration.CodeVersion = ApplicationVersion.Value;
            return result;
        }

        private static readonly Lazy<string> ApplicationVersion = new Lazy<string>(() =>
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;

        });

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="method">The method, referring to the caller.</param>
        /// <param name="file">The file, referring to the caller.</param>
        /// <param name="lineNumber">The line number, referring to the caller.</param>
        /// <remarks>This should be made configurable and pluggable. For now these are test-logging recipient.</remarks>

        public static bool Log(
            this Exception exception,
            [CallerMemberName()]string method = null,
            [CallerFilePath()]string file = null,
            [CallerLineNumber()]int lineNumber = 0)
        {
            Trace.TraceWarning("Logging exception : {0}", exception);

            var anySuccess = false;

            try
            {
                //Send to Rollbar
                RollbarClient.Value.SendException(exception);

                anySuccess = true;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Logging to Rollbar failed : {0}", ex);
            }

            return anySuccess;

            //if (!anySuccess)
            //{
            //    Interaction.MsgBox(string.Format("Reporting an error failed. Please contact the developer regarding the error. Error details : {0}", exception), MsgBoxStyle.OkOnly, "Failed to report error");
            //}
        }
    }
}
