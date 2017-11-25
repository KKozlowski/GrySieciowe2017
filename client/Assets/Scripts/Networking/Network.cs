using System;
using System.Collections;
using System.Collections.Generic;

public class Network
{
    private class ReliableEventResponseListener : IEventListener
    {
        private Action<int> callback;
        public ReliableEventResponseListener(Action<int> callback)
        {
            this.callback = callback;
        }

        public EventType GetEventType()
        {
            return EventType.ReliableEventResponse;
        }

        public bool Execute(EventBase e)
        {
            ReliableEventBase reb = e as ReliableEventBase;
            Network.Log("Response received");
            if (reb!=null)
            {
                
                callback(reb.m_reliableEventId);
                return true;
            }
            return false;
        }
    }

    public class ServerManager
    {
        NetServer m_server;

        public World World { get; private set; }

        private int m_lastReliableEventId = 0;

        private class REventIdPair
        {
            public int id;
            public ReliableEventBase eventToSend;

            public REventIdPair(int id, ReliableEventBase e)
            {
                this.id = id;
                eventToSend = e;
            }
        }

        private Dictionary<int, REventIdPair> m_reliablesToRepeat
            = new Dictionary<int, REventIdPair>();

        private ReliableEventResponseListener m_responseListener;

        public ServerManager( NetServer server )
        {
            m_server = server;
            World = new World();
            World.Init();

            m_responseListener = new ReliableEventResponseListener(ReleaseReliable);
            Network.AddListener(m_responseListener);
        }

        public void Send( EventBase e, int connectionId, bool reliable = false, bool internalResend = false )
        {
            ByteStreamWriter stream = new ByteStreamWriter();
            stream.WriteByte( reliable ? ( byte )MsgFlags.ReliableEvent : ( byte )MsgFlags.UnreliableEvent );
            stream.WriteByte( e.GetId() );

            e.Serialize( stream );
            m_server.GetConnectionById(connectionId).Send( stream.GetBytes() );
            if (reliable && !internalResend && e is ReliableEventBase) {
                ReliableEventBase reb = e as ReliableEventBase;
                m_reliablesToRepeat[reb.m_reliableEventId] = new REventIdPair(connectionId, reb);
            }
        }

        public void ResendRemainingReliables()
        {
            foreach (var pair in m_reliablesToRepeat)
            {
                Send(pair.Value.eventToSend, pair.Value.id, true, true);
            }
        }

        public void ReleaseReliable(int id) {
            m_reliablesToRepeat.Remove(id);
        }

        public int GetNewReliableEventId() {
            return m_lastReliableEventId++;
        }
    }

    public class ClientManager
    {
        NetClient m_client;

        private int m_lastReliableEventId = 0;

        public int ConnectionId { get { return m_client.ConnectionId; } }

        private Dictionary<int, ReliableEventBase> m_reliablesToRepeat 
            = new Dictionary<int, ReliableEventBase>();

        private ReliableEventResponseListener m_responseListener;

        public ClientManager( NetClient client )
        {
            m_client = client;

            m_responseListener = new ReliableEventResponseListener(ReleaseReliable);
            Network.AddListener(m_responseListener);
        }

        public void Send( EventBase e, bool reliable = false, bool internalResend = false )
        {
            ByteStreamWriter stream = new ByteStreamWriter();
            stream.WriteByte( reliable ? (byte)MsgFlags.ReliableEvent : (byte)MsgFlags.UnreliableEvent );
            stream.WriteByte( e.GetId() );
            e.Serialize( stream );
            m_client.Send( stream.GetBytes() );
            if (reliable && !internalResend && e is ReliableEventBase)
            {
                ReliableEventBase reb = e as ReliableEventBase;
                m_reliablesToRepeat[reb.m_reliableEventId] = reb;
            }
        }

        public void ResendRemainingReliables() {
            foreach (var pair in m_reliablesToRepeat) {
                Send(pair.Value, true, true);
            }
        }

        public void ReleaseReliable(int id)
        {
            m_reliablesToRepeat.Remove(id);
        }

        public int GetNewReliableEventId() {
            return m_lastReliableEventId++;
        }

        public void Shutdown() {
            m_client.Shutdown();
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

    public static System.Action<object> Log;

    static Network()
    {
        m_network = new Network();
    }

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

        if (Log == null)
            Log = (object o) => { System.Console.WriteLine(o.ToString()); };

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