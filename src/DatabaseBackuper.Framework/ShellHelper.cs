using System.Diagnostics;
using Serilog;

namespace DatabaseBackuper.Framework;

public static class ShellHelper
{
    public static Task<int> Bash(string cmd, ILogger logger)
    {
        var source = new TaskCompletionSource<int>();
        var escapedArgs = cmd.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };
        process.Exited += (sender, args) =>
        {
            var errors = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(errors))
            {
                logger.Error(errors);
            }

            var output = process.StandardOutput.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(output))
            {
                logger.Information(output);
            }
       
            if (process.ExitCode == 0)
            {
                source.SetResult(0);
            }
            else
            {
                source.SetException(new Exception($"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
            }

            process.Dispose();
        };

        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            logger.Error(e, "Command {} failed", cmd);
            source.SetException(e);
        }

        return source.Task;
    }
}