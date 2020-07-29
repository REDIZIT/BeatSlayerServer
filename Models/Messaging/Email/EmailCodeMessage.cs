using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Models.Messaging.Email
{
    public class EmailCodeMessage : IEmailMessage
    {
        public string Code { get; }

        public EmailCodeMessage(string Code)
        {
            this.Code = Code;
        }

        public string GetMessage()
        {
            return Code;
        }
    }
}
