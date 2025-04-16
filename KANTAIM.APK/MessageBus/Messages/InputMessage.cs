using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.APK.MessageBus.Messages
{
    public class InputMessage : BaseMessage
    {
        public string Code { get; set; }
    }
}
