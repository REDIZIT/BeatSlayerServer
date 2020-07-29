using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Models.Messaging.Email
{
    public interface IEmailMessage
    {
        string GetMessage();
    }
}
