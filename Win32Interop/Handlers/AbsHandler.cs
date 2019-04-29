using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32Interop.Handlers
{
    abstract class AbsHandler
    {
        public abstract string Type { get; }
        public abstract Task<string> Handle(string data);
    }
}
