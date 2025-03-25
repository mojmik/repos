using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using Microsoft.Win32.TaskScheduler;

namespace mCompWardenService
{
    public partial class CompWardenService : ServiceBase
    {
        private System.Timers.Timer _timer;
        private readonly string systemTaskName = "mCompWardenSystem";  // Actual task name
        private readonly string userTaskName = "mCompWardenUser";
        private readonly string logFilePath = @"c:\it\compwarden\compwardenservice.txt";
        private readonly string cmdFilePath = @"c:\it\compwarden\compwardenservice-cmd.txt";
        private readonly string taskExe = @"c:\it\compwarden\mCompWarden2.exe";
        private readonly string srcExe = @"\\rentex.intra\SYSVOL\rentex.intra\scripts\mcompwarden2\mCompWarden2.exe";

        public CompWardenService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogEvent("Service started.");
            // Set the timer interval (60 seconds)
            _timer = new System.Timers.Timer(60000);
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // If a command file exists, process its commands first.
            if (File.Exists(cmdFilePath))
            {
                try
                {
                    // Read commands and normalize (trim and lowercase)
                    string[] commands = File.ReadAllLines(cmdFilePath)
                                             .Select(c => c.Trim().ToLower())
                                             .ToArray();

                    bool updateCommand = commands.Any(c => c == "update");
                    bool stopCommand = commands.Any(c => c == "stop");

                    if (updateCommand)
                    {
                        LogEvent("Update command detected. Stopping tasks, updating executable, then restarting tasks.");

                        // Stop both tasks synchronously.
                        StopTaskSynchronously(systemTaskName);
                        StopTaskSynchronously(userTaskName);

                        // Overwrite task executable with the source executable.
                        try
                        {
                            File.Copy(srcExe, taskExe, true);
                            LogEvent("Task executable updated successfully from source.");
                        }
                        catch (Exception ex)
                        {
                            LogEvent($"Error updating task executable: {ex}");
                        }

                        // Delete the command file after a successful update.
                        try
                        {
                            File.Delete(cmdFilePath);
                            LogEvent("Command file deleted after update.");
                        }
                        catch (Exception ex)
                        {
                            LogEvent($"Error deleting command file after update: {ex}");
                        }

                        // Restart both tasks.
                        RestartTaskSynchronously(systemTaskName);
                        RestartTaskSynchronously(userTaskName);

                        // Exit the timer tick after processing update.
                        return;
                    }
                    else if (stopCommand)
                    {
                        LogEvent("Stop command detected. Stopping tasks and leaving them stopped until command file is removed.");

                        // Stop both tasks and do not restart them.
                        StopTaskSynchronously(systemTaskName);
                        StopTaskSynchronously(userTaskName);

                        // Exit the timer tick.
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LogEvent($"Error processing command file: {ex}");
                }
            }

            // Normal operation: check and restart tasks if needed.
            Thread thread1 = new Thread(() => CheckAndRestartTask(systemTaskName));
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();

            Thread thread2 = new Thread(() => CheckAndRestartTask(userTaskName));
            thread2.SetApartmentState(ApartmentState.STA);
            thread2.Start();
        }

        /// <summary>
        /// Stops the specified task and waits for the operation to complete.
        /// </summary>
        private void StopTaskSynchronously(string taskName)
        {
            Thread stopThread = new Thread(() => CheckAndStopTask(taskName));
            stopThread.SetApartmentState(ApartmentState.STA);
            stopThread.Start();
            stopThread.Join();
        }

        /// <summary>
        /// Restarts the specified task and waits for the operation to complete.
        /// </summary>
        private void RestartTaskSynchronously(string taskName)
        {
            Thread restartThread = new Thread(() => CheckAndRestartTask(taskName));
            restartThread.SetApartmentState(ApartmentState.STA);
            restartThread.Start();
            restartThread.Join();
        }

        /// <summary>
        /// Checks if a task is running; if not, restarts it.
        /// </summary>
        private void CheckAndRestartTask(string taskName)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    var task = ts.GetTask(taskName);
                    if (task == null)
                    {
                        LogEvent($"Task '{taskName}' does not exist.");
                    }
                    else if (task.State != TaskState.Running)
                    {
                        LogEvent($"Task '{taskName}' was not running. Restarting it.");
                        task.Run();
                    }
                    else
                    {
                        // Optionally log that the task is already running.
                        // LogEvent($"Task '{taskName}' is running.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogEvent($"Error in CheckAndRestartTask for '{taskName}': {ex}");
            }
        }

        /// <summary>
        /// Checks if a task is running; if it is, stops it.
        /// </summary>
        private void CheckAndStopTask(string taskName)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    var task = ts.GetTask(taskName);
                    if (task == null)
                    {
                        LogEvent($"Task '{taskName}' does not exist.");
                    }
                    else if (task.State == TaskState.Running)
                    {
                        LogEvent($"Task '{taskName}' is running. Stopping it.");
                        task.Stop();
                    }
                    else
                    {
                        LogEvent($"Task '{taskName}' is not running. No need to stop.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogEvent($"Error in CheckAndStopTask for '{taskName}': {ex}");
            }
        }

        protected override void OnStop()
        {
            LogEvent("Service stopped.");
            _timer?.Stop();
            _timer?.Dispose();
        }

        private void LogEvent(string message)
        {
            try
            {
                // Ensure the log directory exists.
                string directory = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Append the log message with a timestamp.
                string logMessage = $"{DateTime.Now:G} - {message}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception)
            {
                // In a service, logging failures can be ignored or handled as needed.
            }
        }
    }
}
