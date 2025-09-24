using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Util
{
    public class ErrorMessage
    {
        private bool _isError { get; set; }
        private string _message { get; set; }

        public ErrorMessage(bool isError, string message)
        {
            _isError = isError;
            _message = message;
        }

        /// <summary>
        /// This class should only be used to return
        /// true to a caller who expects an ErrorMessage,
        /// use 2 arguments constructor otherwise
        /// </summary>
        /// <param name="isError"></param>
        public ErrorMessage(bool isError)
        {
            _isError = isError;
        }
    }
}