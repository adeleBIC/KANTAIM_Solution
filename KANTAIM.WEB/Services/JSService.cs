using KANTAIM.WEB.MessageBus.Messages;
using Microsoft.JSInterop;

namespace KANTAIM.WEB.Services
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
