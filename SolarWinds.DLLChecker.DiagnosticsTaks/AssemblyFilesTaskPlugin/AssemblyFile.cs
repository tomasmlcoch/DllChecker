namespace SolarWinds.DLLChecker.DiagnosticsTasks.AssemblyFilesTaskPlugin
{
    using SolarWinds.DLLChecker.Backend;
    using SolarWinds.DLLChecker.DiagnosticsTasksContract;
    using System;
    using System.Runtime.CompilerServices;

    public class AssemblyFile : IEquatable<AssemblyFile>, IPatternObject
    {
        public bool Equals(AssemblyFile other)
        {
            if (other == null)
            {
                return false;
            }
            return (this.Path.Equals(other.Path) && this.Version.Equals(other.Version));
        }

        public override bool Equals(object obj)
        {
            AssemblyFile other = obj as AssemblyFile;
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return (this.Path.GetHashCode() ^ this.Version.GetHashCode());
        }

        public SolarWinds.DLLChecker.Backend.Module Module { get; set; }

        public string Path { get; set; }

        public string Version { get; set; }
    }
}

