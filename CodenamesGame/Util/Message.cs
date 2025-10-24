using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Util
{
    public class Message
    {
        public bool IsSuccess { get; set; }
        public string Content { get; set; }

        public Message()
        {

        }
        public Message(bool isError, string content)
        {
            IsSuccess = isError;
            Content = content;
        }
    }
}