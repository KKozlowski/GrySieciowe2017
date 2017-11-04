using System.Collections;
using System.Collections.Generic;

public class Network
{
    static Network m_network;

    NetClient m_client;
    NetServer m_server;

    bool m_isServer;

    public static void Init( bool isServer )
    {
        System.Diagnostics.Debug.Assert( m_network == null );

        m_network = new Network();
        m_network.m_isServer = isServer;

        if ( isServer )
        {
            NetServer server = new NetServer();
            server.Start( 1337 );
            m_network.m_server = server;
        }
        else
        {
            NetClient client = new NetClient();
            client.Connect( "127.0.0.1", 1337 );
            m_network.m_client = client;
        }
    }

    public static void Send( EventBase e, bool reliable, PlayerSession target )
    {
        System.Diagnostics.Debug.Assert( m_network == null );
        System.Diagnostics.Debug.Assert( m_network.m_isServer );

        m_network.InternalSendUnreliable( e, target );
    }

    public static void Send( EventBase e, bool reliable )
    {
        System.Diagnostics.Debug.Assert( m_network == null );
        System.Diagnostics.Debug.Assert( !m_network.m_isServer );

        m_network.InternalSendUnreliable( e );
    }

    private void InternalSendUnreliable( EventBase e, PlayerSession target )
    {
        ByteStreamWriter stream = new ByteStreamWriter();
        e.Serialize( stream );
        target.GetConnection().Send( stream.GetBytes() );
    }

    private void InternalSendUnreliable( EventBase e )
    {
        ByteStreamWriter stream = new ByteStreamWriter();
        e.Serialize( stream );
        //m_client.Send( stream.GetBytes() );
    }
}