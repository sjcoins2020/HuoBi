using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jli.Common
{
    public class JyEventArgs : EventArgs
    {
        public JyEventArgs(JyFx message)
        {
            this.Message = message;
        }

        public JyFx Message { get; set; }
    }
}
