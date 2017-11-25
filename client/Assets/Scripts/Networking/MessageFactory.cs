using System.Net;

public enum MsgFlags : byte
{
    ConnectionRequest = 1 << 0,
    UnreliableEvent = 1 << 1,
    ReliableEvent = 1 << 2,
    State = 1 << 3
}

public class MessageDeserializer
{
    ByteStreamReader m_stream;

    public const int c_maxPacketSize = 512;

    EventsFactory m_events;
    MessageDispatcher m_dispatcher;

    IHanshakable m_connectionMessagesReceiver;

    public MessageDeserializer()
    {
        m_events = new EventsFactory();
    }

    public void Init( IHanshakable handshake, MessageDispatcher dispatcher )
    {
        m_dispatcher = dispatcher;
        m_connectionMessagesReceiver = handshake;
    }

    public bool HandleData( byte[] data, IPEndPoint endpoint = null )
    {
        System.Diagnostics.Debug.Assert( data.Length < c_maxPacketSize );
        m_stream = new ByteStreamReader( data );

        byte flags = m_stream.ReadByte();
        if ( HandleEvent( m_stream, flags ) )
        {
            return true;
        }
        if ( HandleConnectionMsg( m_stream, flags, endpoint ) )
        {
            return true;
        }

        return false;
    }

    bool HandleEvent( ByteStreamReader stream, byte flags )
    {
        if ( HasFlag( flags, ( byte )MsgFlags.UnreliableEvent ) )
        {
            byte eventType = stream.ReadByte();
            EventBase evnt = m_events.CreateEvent( eventType );
            //Network.Log("Event type: " + evnt.GetEventType());
            evnt.Deserialize( stream );
            m_dispatcher.PushEvent( evnt );
            return true;
        }
        else if ( HasFlag( flags, ( byte )MsgFlags.ReliableEvent ) )
        {
            byte eventType = stream.ReadByte();
            EventBase evnt = m_events.CreateEvent(eventType);

            evnt.Deserialize(stream);
            m_dispatcher.PushEvent(evnt);

            return true;
        }

        return false;
    }

    bool HandleConnectionMsg( ByteStreamReader stream, byte flags, IPEndPoint endpoint )
    {
        if ( HasFlag( flags, ( byte )MsgFlags.ConnectionRequest ) )
        {
            byte content = stream.ReadByte();
            m_connectionMessagesReceiver.HandleConnectionData( ( HandshakeMessage )content, stream, endpoint );
            return true;
        }

        return false;
    }

    bool HasFlag( byte flag, byte value )
    {
        return ( flag & value ) != 0;
    }
}