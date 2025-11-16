using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network.EventArguments
{
    /// <summary>
    /// Arguments for API operation events that return a message.
    /// </summary>
    public class OperationMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
