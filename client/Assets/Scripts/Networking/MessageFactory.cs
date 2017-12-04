using System.Net;

/// <summary>
/// Basic types of messages that san be sent or received.
/// </summary>
public enum MsgFlags : byte
{
    /// <summary>
    /// Three-Way handshake process data
    /// </summary>
    ConnectionRequest = 1 << 0,
    /// <summary>
    /// Event that doesn't need confirmation
    /// </summary>
    UnreliableEvent = 1 << 1,
    /// <summary>
    /// Event that is spammed until it gets a confirmation.
    /// </summary>
    ReliableEvent = 1 << 2,
}

/// <summary>
/// Identifies received byte arrays as ConnectionRequests, UnreliableEvents or ReliableEvents, calls
/// event construction in EventFactory, dispatches connection messages and calls event dispatch.
/// </summary>
public class MessageDeserializer
{
    /// <summary>
    /// Helper field for currently read bytestream.
    /// </summary>
    ByteStreamReader m_stream;

    /// <summary>
    /// Maximum package size that can be deserialized.
    /// </summary>
    public const int c_maxPacketSize = 512;

    /// <summary>
    /// Creates events from identified bytestreams
    /// </summary>
    EventsFactory m_events;
    /// <summary>
    /// Sends deserialized events to corresponding listeners.
    /// </summary>
    MessageDispatcher m_dispatcher;

    /// <summary>
    /// Object that will receive ConnectionRequest messages
    /// </summary>
    IHanshakable m_connectionMessagesReceiver;

    public MessageDeserializer()
    {
        m_events = new EventsFactory();
    }

    /// <summary>
    /// Initializes the m_connectionMessagesReceiver and m_dispatcher fields.
    /// </summary>
    /// <param name="handshake">The handshake events receiver.</param>
    /// <param name="dispatcher">The message dispatcher.</param>
    public void Init( IHanshakable handshake, MessageDispatcher dispatcher )
    {
        m_dispatcher = dispatcher;
        m_connectionMessagesReceiver = handshake;
    }

    /// <summary>
    /// Takes care of data from a byte array, calling all the necessary deserializations and dispatches.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="endpoint">The data source.</param>
    /// <returns><c>true</c> if data was successfully handles, <c>false</c> otherwise.</returns>
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

    /// <summary>
    /// Called when byte array is identified as an event. Deserializes and dispatches it.
    /// </summary>
    /// <param name="stream">Data stream.</param>
    /// <param name="flags">Stream flags (reliable or unreliable event?).</param>
    /// <returns><c>true</c> if stream was handled correctly, <c>false</c> otherwise.</returns>
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

    /// <summary>
    /// Called when stream is identified as a ConnectionRequest object.
    /// </summary>
    /// <param name="stream">Data stream.</param>
    /// <param name="flags">Stream flags.</param>
    /// <param name="endpoint">Data source.</param>
    /// <returns><c>true</c> if stream was handled correctly, <c>false</c> otherwise.</returns>
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