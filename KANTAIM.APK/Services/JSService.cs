using KANTAIM.APK.MessageBus.Messages;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Provider.CalendarContract;

namespace KANTAIM.APK.Services
{
    public class JSService
    {
        IJSRuntime _jSRuntime;

        public JSService(IJSRuntime jSRuntime)
        {
             _jSRuntime = jSRuntime;
        }

        [JSInvokable]
        public static void CodeScanned(string code)
        {
            MessageBus.MessageBus.SendMessage(new InputMessage()
            {
                Code = code
            });
        }
    }
}
