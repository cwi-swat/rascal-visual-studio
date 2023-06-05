using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TextWriter errorWriter = Console.Error;
            errorWriter.WriteLine("wait for input");
            var line = Console.ReadLine();
            Console.WriteLine("Received line: " + line);
            errorWriter.WriteLine("Received line: " + line);
            Console.ReadLine();
        }
    }
}
