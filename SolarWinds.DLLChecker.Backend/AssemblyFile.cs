using System;

namespace SolarWinds.DLLChecker.Backend
{
    public class AssemblyFile : IEquatable<AssemblyFile>
    {
        public string Path { get; set; }

        public string Version { get; set; }

        public Module Module { get; set; }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var other = obj as AssemblyFile;
            if ((System.Object)other == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Path.Equals(other.Path) && Version.Equals(other.Version));
        }

        public bool Equals(AssemblyFile other)
        {
            // If parameter is null return false:
            if ((object)other == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Path.Equals(other.Path) && Version.Equals(other.Version));
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode() ^ Version.GetHashCode();
        }

       
    }
}
