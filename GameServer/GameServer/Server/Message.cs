using GameServer;
using System;

public enum MessageID
{
    Error = -1,   
    Test = 1000,
    Login = 1001,
}
public class MessageArgs
{
    public Connection sender;
    public MessageID id;
    public object message;
}

public class Message : Singleton<Message>
{
    public MessageID GetMessageID<T>()
    {
        if (!Enum.TryParse(typeof(T).Name.Replace("Res", ""), out MessageID id))
        {
            id = MessageID.Error;
        }
        return id;
    }

    public void Distribute(MessageArgs args)
    {
        switch (args.id)
        {
            case MessageID.Test:
                MessageRegister.Instance.Fire(args.sender, args.id, args.message as Test);
                break;
            case MessageID.Login:
                MessageRegister.Instance.Fire(args.sender, args.id, args.message as Login);
                break;
        }
    }
}