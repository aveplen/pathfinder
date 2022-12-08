using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStar
{
    [Serializable]
    public class UnreachableException : Exception
    {
        public UnreachableException() { }

        public UnreachableException(string message) : base(message) { }
    }
}
