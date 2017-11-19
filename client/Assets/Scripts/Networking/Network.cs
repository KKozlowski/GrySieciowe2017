using System.Collections;
using System.Collections.Generic;

public class Network
{
    public class ServerManager
    {
        NetServer m_server;

        public World World { get; private set; }

        public ServerManager( NetServer server )
        {
            m_server = server;
            World = new World();
            World.Init();
        }

        public void Send( EventBase e, PlayerSession target, bool reliable = false )
        {
            ByteStreamWriter stream = new ByteStreamWriter();
            stream.WriteByte( reliable ? ( byte )MsgFlags.ReliableEvent : ( byte )MsgFlags.UnreliableEvent );
            stream.WriteByte( e.GetId() );

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
            stream.WriteByte( e.GetId() );
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
        if ( isServer )
        {
            InitAsServer(1337);
        }
        else
        {
            InitAsClient("127.0.0.1", 2137, 1337);
        }
    }

    private static void InitBasic(bool isServer) {
        System.Diagnostics.Debug.Assert(m_network == null);

        m_network = new Network();
        m_network.m_isServer = isServer;

        m_network.m_dispatcher = new MessageDispatcher();
        m_network.m_deserializer = new MessageDeserializer();
    }

    public static void InitAsServer(int port) {
        InitBasic(true);
        NetServer server = new NetServer();
        m_network.m_deserializer.Init(server, m_network.m_dispatcher);

        server.SetDeserializer(m_network.m_deserializer);
        server.Start(port);

        ServerManager manager = new ServerManager(server);
        m_network.m_server = manager;
    }

    public static void InitAsClient(string serverIp, int listenPort, int receivePort) {
        InitBasic(false);
        NetClient client = new NetClient();
        m_network.m_deserializer.Init(client, m_network.m_dispatcher);

        client.Connect(serverIp, listenPort, receivePort);
        client.SetDeserializer(m_network.m_deserializer);

        ClientManager manager = new ClientManager(client);
        m_network.m_client = manager;
    }

    public static void AddListener( IEventListener listener )
    {
        m_network.m_dispatcher.AddListener( listener );
    }

    public static void RemoveListener( IEventListener listener )
    {
        m_network.m_dispatcher.RemoveListener( listener );
    }
}