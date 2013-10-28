using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarWinds.DLLChecker.Backend
{
   public class Module
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool? HasPattern { get; set; }

    }
}
