using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;
using StreamJsonRpc;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client");
            Process childProcess = Process.Start(new ProcessStartInfo("E:\\architectural-erosion\\rascal-visual-studio\\rascal-visual-studio-analysis-client\\ErrorList\\Server\\bin\\Debug\\Server.exe")
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            });
            Console.ReadKey();
        }
    }
}
