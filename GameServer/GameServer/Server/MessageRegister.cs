using System;
using System.Collections.Generic;
namespace GameServer
{
    class MessageRegister : Singleton<MessageRegister>
    {
        public delegate void Handler<T>(Connection sender, T message);
        private Dictionary<MessageID, System.Delegate> MessageEvents = new Dictionary<MessageID, System.Delegate>();

        public void Subscribe<T>(MessageID id, Handler<T> handler)
        {
            if (!MessageEvents.ContainsKey(id))
            {
                MessageEvents.Add(id, handler);
            }
            else
            {
                MessageEvents[id] = (Handler<T>)MessageEvents[id] + handler;
            }
        }

        public void Unsubscribe<T>(MessageID id, Handler<T> handler)
        {
            if (MessageEvents.ContainsKey(id))
            {
                MessageEvents[id] = (Handler<T>)MessageEvents[id] - handler;
                if (MessageEvents[id] == null)
                {
                    MessageEvents.Remove(id);
                }
            }
        }

        public void Fire<T>(Connection sender, MessageID id, T msg)
        {
            if (MessageEvents.TryGetValue(id, out System.Delegate d))
            {
                Handler<T> handler = (Handler<T>)d;
                handler(sender, msg);
            }
        }
    }
}

