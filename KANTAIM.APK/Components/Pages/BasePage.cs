using KANTAIM.APK.MessageBus.Messages;
using KANTAIM.APK.MessageBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace KANTAIM.APK.Components.Pages
{
    public abstract class BasePage : ComponentBase, IListener<InputMessage>, IDisposable
    {
        protected BasePage()
        {
            MessageBus.MessageBus.Subscribe<InputMessage>(this);
        }
        public void Dispose()
        {
            MessageBus.MessageBus.UnSubscribe<InputMessage>(this);
        }

        public abstract void OnMessageReceived(InputMessage msg);
    }
}
