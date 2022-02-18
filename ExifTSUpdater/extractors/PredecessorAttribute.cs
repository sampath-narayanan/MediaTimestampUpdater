using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.ExifTSUpdater
{
    [ AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false ) ]
    public class PredecessorAttribute : Attribute
    {
        public PredecessorAttribute(
            Type? predecessor
        )
        {
            Predecessor = predecessor;
        }

        public Type? Predecessor { get; }
    }
}
