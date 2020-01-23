using System;
using System.Collections.Generic;
using System.Text;

namespace Benner.Messaging.Common
{
    public class MessageBase
    {
        public string Body { get; set; }

        public List<Exception> ExceptionList { get; set; }

        public MessageBase() => ExceptionList = new List<Exception>();
    }
}
