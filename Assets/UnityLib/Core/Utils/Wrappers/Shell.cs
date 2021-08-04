using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UnityEngine;
using System.IO;

namespace Nettle {
    public static class Shell {
        public const string DefaultOutputPath = "/ShellOutput.txt";

        public class ProcessResult {
            public int ErrorCode;
            public string StdOut;
            public string StdErr;
            public bool Success {
                get { return ErrorCode == 0; }
            }
        }

        public class CommandProperties
        {
            public bool OutputToFile = false;
            public string OutputPath;
            public CommandProperties()
            {
                OutputPath = Application.persistentDataPath + DefaultOutputPath;
            }
        }


        public static string CurrentDirectory {
            get; set;
        }


        public static ProcessResult Execute(string command, CommandProperties properties = null) {
            //Console.WriteLine("Trying to execute command: " + command);
            if (properties == null)
            {
                properties = new CommandProperties();
            }
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            string fullCommand;
            if (!string.IsNullOrEmpty(CurrentDirectory)) {
                fullCommand = "cd /d " + CurrentDirectory + " & " + command;
            }
            else {
                fullCommand = command;
            }
            startInfo.Arguments = "/C " + fullCommand;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;


            StringBuilder error = new StringBuilder();
            StringBuilder output = null;
            FileStream outputStream = null;
            if (properties.OutputToFile)
            {
                outputStream = File.OpenWrite(properties.OutputPath);                
            }else
            {
                output = new StringBuilder();
            }

            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false)) {
                process.OutputDataReceived += (sender, e) => {
                    if (e.Data == null) {
                        outputWaitHandle.Set();
                    } else {
                        if (!properties.OutputToFile)
                        {
                            output.AppendLine(e.Data);
                        }else
                        {
                            byte[] data = Encoding.ASCII.GetBytes(e.Data + "\n");
                            outputStream.Write(data,0,data.Length);
                        }
                    }
                };
                process.ErrorDataReceived += (sender, e) => {
                    if (e.Data == null) {
                        errorWaitHandle.Set();
                    } else {
                        error.AppendLine(e.Data);
                    }
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                outputWaitHandle.WaitOne();
                errorWaitHandle.WaitOne();
            }
            ProcessResult result = new ProcessResult();
            result.ErrorCode = process.ExitCode;
            if (properties.OutputToFile)
            {
                outputStream.Close();
            }else
            {
                result.StdOut = output.ToString();
            }
            result.StdErr = error.ToString();
            return result;
        }
    }
}
