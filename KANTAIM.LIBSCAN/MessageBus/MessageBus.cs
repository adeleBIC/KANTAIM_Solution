using KANTAIM.LIBSCAN.MessageBus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.LIBSCAN.MessageBus
{
    public static class MessageBus
    {
        static List<WeakReference> _subscribers = new List<WeakReference>();

        public static void Subscribe<T>(IListener instance) where T : BaseMessage
        {
            _subscribers.Add(new WeakReference(instance));
        }

        public static void UnSubscribe<T>(IListener instance) where T : BaseMessage
        {
            var weakReference = _subscribers.SingleOrDefault(wr => wr.IsAlive && wr.Target == instance);
            if (weakReference != null) _subscribers.Remove(weakReference);
        }


        public static void SendMessage<T>(T msg) where T : BaseMessage
        {
            foreach (var subscriber in _subscribers.ToList())
            {
                if (subscriber.IsAlive)
                {
                    var canContinue = subscriber.Target.GetType().GetInterfaces().Any(i => i.GenericTypeArguments.Length > 0 && i.GenericTypeArguments[0].IsAssignableFrom(typeof(T)));

                    if (canContinue)
                        ((IListener<T>)subscriber.Target).OnMessageReceived(msg);
                }
                else _subscribers.Remove(subscriber); // Remove dead references
            }
        }
    }
}
