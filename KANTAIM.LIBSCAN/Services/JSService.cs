using KANTAIM.LIBSCAN.MessageBus.Messages;
using Microsoft.JSInterop;

namespace KANTAIM.LIBSCAN.Services
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
