using KANTAIM.LIBSCAN.MessageBus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.LIBSCAN.MessageBus
{

    public interface IListener
    {
    }
    public interface IListener<T> : IListener where T : BaseMessage
    {
        public void OnMessageReceived(T msg);
    }
}
