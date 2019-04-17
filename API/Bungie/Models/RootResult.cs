using System;
using System.Collections.Generic;
using System.Text;

namespace API.Bungie.Models
{
    public class RootObject
    {
        public int ErrorCode { get; set; }
        public int ThrottleSeconds { get; set; }
        public string ErrorStatus { get; set; }
        public string Message { get; set; }
        public MessageData MessageData { get; set; }
    }
    public class MessageData { }

}
