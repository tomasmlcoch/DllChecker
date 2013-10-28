using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SolarWinds.DLLChecker.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";

            bool runInConsole = false;

            if ((args.Length == 1) && ((args[0][0] == '/') || (args[0][0] == '-')))
                runInConsole = (args[0].Substring(1).ToLower() == "noservice");

            // set the current directory so we can reliably find the schemas using a relative path
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (runInConsole)
                new CheckerService().RunInConsole();
            else
                ServiceBase.Run(new ServiceBase[] { new CheckerService() });
          


          
        }
    }
}
