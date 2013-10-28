using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarWinds.DLLChecker.Backend

{
    public class Difference : IEquatable<Difference>
    {
        public string Title{ get; set; }
        public AssemblyFile ExpectedFile { get; set; }
        public string FoundVersion { get; set; }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var other = obj as Difference;
            
            if (other == null)
            {
                return false;
            }

            return Equals(other);
         }

        public bool Equals(Difference other)
        {
            // If parameter is null return false:
            if ((object)other == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Title.Equals(other.Title) && ExpectedFile.Equals(other.ExpectedFile) && FoundVersion.Equals(other.FoundVersion));
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode() ^ ExpectedFile.GetHashCode() ^ FoundVersion.GetHashCode();
        }
    }


}
