using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading.Tasks;
using Nerdbank.Streams;
using StreamJsonRpc;

namespace Server
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Client");
            var childProcess = new ProcessStartInfo("E:\\architectural-erosion\\rascal-visual-studio\\rascal-visual-studio-analysis-client\\ErrorList\\Server\\bin\\Debug\\Server.exe", "");
            childProcess.UseShellExecute = false;
            childProcess.RedirectStandardInput = true;
            childProcess.RedirectStandardOutput = true;
            var process = Process.Start(childProcess);
            var stdioStream = FullDuplexStream.Splice(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
            await ActAsRpcClientAsync(stdioStream);
            Console.WriteLine("Hit any key to stop the program");
            Console.ReadKey();
        }

        private static async Task ActAsRpcClientAsync(Stream stream)
        {
            Console.WriteLine("Connected. Sending request...");
            var jsonRpc = JsonRpc.Attach(stream);
            int sum = await jsonRpc.InvokeAsync<int>("Add", 3, 5);
            Console.WriteLine($"3 + 5 = {sum}");
        }
    }
}
