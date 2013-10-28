using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SolarWinds.DLLChecker.Backend;

namespace SolarWinds.DLLChecker.Backend.Tests
{
    [TestFixture]
    class DiagnosticsComparerTests
    {
        //[Test]
        //public void Compare_EqualFiles_ReturnsEmptyDictionary()
        //{
        //    var dictionary1 = new Dictionary<string, AssemblyFile>();
        //    var dictionary2 = new Dictionary<string, AssemblyFile>();
        //    dictionary1.Add("file",new AssemblyFile(){Path = "path.path.path",Module = new Module(){Name = "Module",Version = "2.50"},Version = "1.0"});
        //    dictionary2.Add("file", new AssemblyFile() { Path = "path.path.path", Module = new Module() { Name = "Module", Version = "2.50" }, Version = "1.0" });

        //    var comparer = new DiagnosticsComparer();
        //    var result = comparer.Compare(dictionary1,dictionary2,"install","web");
        //    Assert.AreEqual(new List<Difference>(),result);
        //}

        //[Test]
        //public void Compare_DifferentFiles_ReturnsDifferenceDictionary()
        //{
        //    var dictionary1 = new Dictionary<string, AssemblyFile>();
        //    var dictionary2 = new Dictionary<string, AssemblyFile>();


        //    var pattern = new AssemblyFile()
        //        {
        //            Path = "path.path.path",
        //            Module = new Module() {Name = "Module", Version = "2.50"},
        //            Version = "1.50"
        //        };
        //    var pattern1 = new AssemblyFile()
        //        {
        //            Path = "path.path.pth",
        //            Module = new Module() {Name = "Module", Version = "2.50"},
        //            Version = "1.50"
        //        };

        //    var file2 = new AssemblyFile()
        //        {
        //            Path = "path.path.path",
        //            Module = new Module() {Name = "Module", Version = "2.50"},
        //            Version = "1.6"
        //        };

           
            
        //    dictionary1.Add("path.path.path", pattern);
        //    dictionary2.Add("path.path.path", file2);
        //    dictionary1.Add("path.path.pth", pattern1);
            


        //    var expected = new List<Difference>();
        //    expected.Add(new Difference(){ExpectedFile = pattern,FoundVersion = "1.6",Title = "WRONG VERSION"});
        //    expected.Add(new Difference() { ExpectedFile = pattern1, FoundVersion = "-", Title = "MISSING" });


        //    var comparer = new DiagnosticsComparer();
        //    var result = comparer.Compare(dictionary1, dictionary2,"","").ToArray();
           
        //    Assert.AreEqual(expected as IEnumerable<Difference>, result);
        //}
    }
}
