using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using SolarWinds.DLLChecker.Backend;

namespace SolarWinds.DLLChecker.Manager
{
    class Manager
    {
       /* private readonly PatternsManager patternsManager;
        
        public Manager()
        {
         patternsManager = new PatternsManager();
        }

        public void Run()
        {
            bool end = false;

            

            ConsoleKeyInfo keyPressed;

            do
            {
                Console.Clear();
                Console.WriteLine("Solarwinds DLL Checker - Manager ");
                ShowMenu();
                keyPressed = Console.ReadKey(true);

                try
                {
                    switch (keyPressed.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            CreatePattern();
                            break;

                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            ShowPatterns();
                            break;

                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            ShowPatternDetails();
                            break;
                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            DeletePattern();
                            break;
                        case ConsoleKey.D5:
                        case ConsoleKey.NumPad5:
                            DeleteAllPatterns();
                            break;

                        case ConsoleKey.D0:
                        case ConsoleKey.NumPad0:
                            end = true;
                            break;

                        default: 
                            Console.Clear();
                            break;
                    }
                }
                catch (Exception ex)
                {

                    ShowError(ex.Message);
                    PressAnyKeyToContinue();
                    
                }
                
                
            } while (!end);
        }

        private void ShowMenu()
        {
            var builder = new StringBuilder();

            
            builder.AppendLine(" 1 Create new pattern");
            builder.AppendLine(" 2 Show available patterns");
            builder.AppendLine(" 3 Show pattern details");
            builder.AppendLine(" 4 Delete specific pattern");
            builder.AppendLine(" 5 Delete all patterns");
            builder.AppendLine(" 0 Quit");

            Console.WriteLine(builder.ToString());
        }

        private void CreatePattern()
        {
            var module = new Module();
            string filePath = null;

            Console.Clear();
            Console.WriteLine("- Creating new patter -");
            Console.Write("Module name: ");
            module.Name = Console.ReadLine();
            Console.Write("Module version: ");
            module.Version = Console.ReadLine();
            Console.Write("Path to diagnostics: ");
            filePath = Console.ReadLine();
            try
            {
                patternsManager.CreatePattern(filePath, module);
            }
            catch (Exception ex)
            {

                ShowError(ex.Message);
                PressAnyKeyToContinue();
                return;
            }
            

            Console.WriteLine(string.Format("Pattern for module {0} {1} was successfully created",module.Name,module.Version));
            PressAnyKeyToContinue();

           }

        private void ShowPatternDetails()
        {
            var module = new Module();
            Console.Clear();
            Console.WriteLine(" -Pattern details- ");
            Console.Write("Module name: ");
            module.Name = Console.ReadLine();
            Console.Write("Module version: ");
            module.Version = Console.ReadLine();
            IEnumerable<AssemblyFile> list;

            try
            {
               list = patternsManager.GetPatternDetails(module);
            }
            catch (Exception ex)
            {
                
                ShowError(ex.Message);
                PressAnyKeyToContinue();
                return;
            }
            Console.WriteLine();
            if (!list.Any())
            {
                Console.WriteLine("Pattern not found");
                PressAnyKeyToContinue();
                return;
            }
            Console.WriteLine(string.Format("Pattern consists of {0} files",list.Count()));
            Console.WriteLine();
            Console.WriteLine("1 Show all associated files");
            Console.WriteLine("2 Export result to files");
            Console.WriteLine("3 Back to menu");
            var key = Console.ReadKey();

            switch (key.Key)
            {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        ShowFiles(list);
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        ExportResult(module, list);
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        return;
            }
    }

        private void ShowFiles(IEnumerable<AssemblyFile> list)
        {
            foreach (var dllFile in list)
            {
                Console.WriteLine(string.Format(" {0} {1}", dllFile.Path, dllFile.Version));
            }
            PressAnyKeyToContinue();
        }

        private void ExportResult(Module module,IEnumerable<AssemblyFile> list)
        {
            Console.Clear();
            Console.WriteLine(string.Format(" -Export info about {0} {1}- ",module.Name,module.Version));
            Console.Write("Export file path: ");
            var path = Console.ReadLine();
            try
            {
                using (var writer = new StreamWriter(path))
                {
                    writer.WriteLine(string.Format("Files associated with module {0} {1}",module.Name,module.Version));
                    foreach (var file in list)
                    {
                        writer.WriteLine(file.Path+","+file.Version);   
                    }
                    
                }
            }
            catch (Exception ex)
            {
                
                ShowError(ex.Message);
                PressAnyKeyToContinue();
                
            }
            Console.WriteLine("Export successful");
            PressAnyKeyToContinue();

        }

        private void ShowPatterns()
        {
            IEnumerable<Module> patterns = null;
            try
            {
               patterns = patternsManager.GetAvailableModules();
            }
            catch (Exception ex)
            {
                
               ShowError(ex.Message);
                PressAnyKeyToContinue();
                return;
            }
            
            Console.Clear();

            var enumerable = patterns as Module[] ?? patterns.ToArray();

            Console.WriteLine(string.Format(" -Available patterns ({0})- ",enumerable.Count()));
            Console.WriteLine();

            if (!enumerable.Any())
            {
                
                Console.WriteLine("-none-");
            }
            else
            {
                foreach (var pattern in enumerable)
                {
                    Console.WriteLine(string.Format("{0} {1}",pattern.Name,pattern.Version));
                }
            }

            PressAnyKeyToContinue();
            

        }

        private void DeletePattern()
        {
            var module = new Module();
            Console.Clear();
            Console.WriteLine(" -Delete specific pattern- ");
            Console.WriteLine();
            Console.Write("Module name: ");
            module.Name = Console.ReadLine();
            Console.Write("Module version: ");
            module.Version = Console.ReadLine();
            try
            {
                patternsManager.DeletePattern(module);
            }
            catch (Exception ex)
            {
                
                ShowError(ex.Message);
                PressAnyKeyToContinue();
                return;
            }

            Console.WriteLine(string.Format("Pattern for module {0} {1} has been successfuly removed",module.Name,module.Version));
           PressAnyKeyToContinue();
        }

        private void DeleteAllPatterns()
        {
            Console.Clear();
            Console.WriteLine(" -Remove all patterns from database- ");
            Console.WriteLine("This action is permanent");
            Console.WriteLine("Do you really want to continue? (y/n)");
            var yn = Console.ReadKey(true);
            if (yn.Key == ConsoleKey.Y)
            {
                try
                {
                    patternsManager.DeleteAllPatterns();
                }
                catch (Exception ex)
                {
                    
                    ShowError(ex.Message);
                    PressAnyKeyToContinue();
                    return;
                }
                
            }

            
        }

        private void PressAnyKeyToContinue()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
        }

        private void ShowError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: "+ error);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        */
     }
}
