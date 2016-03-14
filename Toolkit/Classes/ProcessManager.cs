/*
 * ProcessManager.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace Toolkit
{
    internal static class ProcessManager
    {
        private const int Timeout = -1;

        public static void KillAllAdbProcesses()
        {
            foreach (var p in Process.GetProcessesByName("adb"))
                p.Kill();
            foreach (var p in Process.GetProcessesByName("fastboot"))
                p.Kill();
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public static string RunProcess(string executable, string arguments)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                using (var outputWaitHandle = new AutoResetEvent(false))
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    var output = new StringBuilder();
                    var error = new StringBuilder();

                    p.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                            output.AppendLine(e.Data);
                    };
                    p.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                            error.AppendLine(e.Data);
                    };

                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    if (p.WaitForExit(Timeout) && outputWaitHandle.WaitOne(Timeout) && errorWaitHandle.WaitOne(Timeout))
                        return error.ToString().Trim().Length.Equals(0) ? output.ToString().Trim() : error.ToString().Trim();

                    return "TIMEOUT";
                }
            }
        }
    }
}
