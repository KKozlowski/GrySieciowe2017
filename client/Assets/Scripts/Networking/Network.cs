using System.Collections;
using System.Collections.Generic;

public class Network
{
    public class ServerManager
    {
        NetServer m_server;

        public ServerManager( NetServer server )
        {
            m_server = server;
        }

        public void Send( EventBase e, PlayerSession target )
        {
            ByteStreamWriter stream = new ByteStreamWriter();
            e.Serialize( stream );
            target.GetConnection().Send( stream.GetBytes() );
        }
    }

    public class ClientManager
    {
        NetClient m_client;

        public ClientManager( NetClient client )
        {
            m_client = client;
        }

        public void Send( EventBase e, bool reliable = false )
        {
            ByteStreamWriter stream = new ByteStreamWriter();
            stream.WriteByte( reliable ? (byte)MsgFlags.ReliableEvent : (byte)MsgFlags.UnreliableEvent );
            e.Serialize( stream );
            m_client.Send( stream.GetBytes() );
        }
    }


    static Network m_network;

    MessageDeserializer m_deserializer;
    MessageDispatcher m_dispatcher;
    ServerManager m_server;
    ClientManager m_client;

    public static ServerManager Server { get { return m_network.m_server; } }
    public static ClientManager Client { get { return m_network.m_client; } }

    bool m_isServer;

    public static void Init( bool isServer )
    {
        System.Diagnostics.Debug.Assert( m_network == null );

        m_network = new Network();
        m_network.m_isServer = isServer;

        m_network.m_dispatcher = new MessageDispatcher();
        m_network.m_deserializer = new MessageDeserializer( m_network.m_dispatcher );

        if ( isServer )
        {
            NetServer server = new NetServer();
            server.Start( 1337 );
            server.SetDeserializer( m_network.m_deserializer );

            ServerManager manager = new ServerManager( server );
            m_network.m_server = manager;
        }
        else
        {
            NetClient client = new NetClient();
            client.Connect( "127.0.0.1", 1337 );
            client.SetDeserializer( m_network.m_deserializer );

            ClientManager manager = new ClientManager( client );
            m_network.m_client = manager;
        }
    }
}