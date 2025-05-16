using KANTAIM.WEB.MessageBus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.WEB.MessageBus
{

    public interface IListener
    {
    }
    public interface IListener<T> : IListener where T : BaseMessage
    {
        public void OnMessageReceived(T msg);
    }
}
