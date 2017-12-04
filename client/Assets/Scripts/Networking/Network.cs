using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// <c>BASE CLASS FOR THE NETWORK LAYER</c>.
/// It needs to be initialized to allow server or client behaviors.
/// </summary>
public class Network
{
    /// <summary>
    /// Listens for responses to reliable events and calls a callback whenever it gets one.
    /// Can be used by both Client and Server.
    /// </summary>
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
            ReliableEventResponse reb = e as ReliableEventResponse;
            //Network.Log("Response received");
            if (reb!=null)
            {
                
                callback(reb.m_reliableEventId);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// A class that provides high-level server behaviors.
    /// </summary>
    public class ServerManager
    {
        /// <summary>
        /// Server connections manager.
        /// </summary>
        NetServer m_server;

        /// <summary>
        /// Gets the server's game world instance.
        /// </summary>
        /// <value>The world.</value>
        public World World { get; private set; }

        private int m_lastReliableEventId = 0;

        private Thread updateThread;

        /// <summary>
        /// Pair of a reliable event, and identifier of a client it should be sent to.
        /// </summary>
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

        private ConcurrentDictionary<int, REventIdPair> m_reliablesToRepeat
            = new ConcurrentDictionary<int, REventIdPair>();

        private ReliableEventResponseListener m_responseListener;

        public ServerManager( NetServer server )
        {
            m_server = server;
            World = new World();
            World.Init();
            m_server.OnDisconnect += OnDisconnect;

            updateThread = new Thread(TimelyCheck);
            updateThread.Start();

            m_responseListener = new ReliableEventResponseListener((int i) => { TryReleaseReliable(i); });
            Network.AddListener(m_responseListener);
        }

        /// <summary>
        /// Responds to a clients's reliable event.
        /// </summary>
        /// <param name="reliableEventId">The reliable event identifier.</param>
        /// <param name="userId">The client identifier.</param>
        public void RespondToReliableEvent(int reliableEventId, int userId)
        {
            ReliableEventResponse response = new ReliableEventResponse();
            response.m_reliableEventId = reliableEventId;
            Send(response, userId, false);
        }

        void OnDisconnect(int id, bool afk)
        {
            World.RemovePlayer(id);
            Network.Log("Player " + id + " DISCONNECTED " + (afk ? "(AFK)" : "(manually)"));
        }

        /// <summary>
        /// Sends an event to a client.
        /// </summary>
        /// <param name="e">Event to send.</param>
        /// <param name="connectionId">Target client's connection id</param>
        /// <param name="reliable">Is it reliable event</param>
        /// <param name="internalResend">If <c>false</c> this event will be added to reliable events queue as a new event</param>
        /// <returns><c>true</c> if the target client is connected</returns>
        public bool Send( EventBase e, int connectionId, bool reliable = false, bool internalResend = false )
        {
            ByteStreamWriter stream = new ByteStreamWriter();
            stream.WriteByte( reliable ? ( byte )MsgFlags.ReliableEvent : ( byte )MsgFlags.UnreliableEvent );
            stream.WriteByte( e.GetId() );

            e.Serialize( stream );
            Connection c = m_server.GetConnectionById(connectionId);
            if (c != null)
            {
                c.Send(stream.GetBytes());
                if (reliable && !internalResend && e is ReliableEventBase)
                {
                    ReliableEventBase reb = e as ReliableEventBase;
                    m_reliablesToRepeat[reb.m_reliableEventId] = new REventIdPair(connectionId, reb);
                }
                return true;
            }
            else
            {
                return false;
            }
            
        }

        /// <summary>
        /// Resends the unresponded reliable events.
        /// </summary>
        public void ResendRemainingReliables()
        {
            List<int> toRemove = new List<int>();
            foreach (var pair in m_reliablesToRepeat)
            {
                if (!Send(pair.Value.eventToSend, pair.Value.id, true, true))
                    toRemove.Add(pair.Key);
            }

            foreach (int i in toRemove)
            {
                TryReleaseReliable(i);
            }
        }

        /// <summary>
        /// Every seconds, it disconnects clients that haven't sent new packages for the last 5 seconds.
        /// Every clients sends a new Input event every frame, so it implies connection error on the client side.
        /// </summary>
        void TimelyCheck() {
            while (true) {
                Thread.Sleep(1000);
                var toDisconnect = World.GetPlayersWithNoNewPackages(5);
                foreach (int i in toDisconnect)
                {
                    m_server.Disconnect(i, true);
                }
            }
        }

        /// <summary>
        /// Removes unreliable event from unconfirmed reliables queue.
        /// </summary>
        /// <param name="id">The event's identifier.</param>
        /// <returns>Operation succeeded</returns>
        public bool TryReleaseReliable(int id)
        {
            REventIdPair val = null;
            return m_reliablesToRepeat.TryRemove(id, out val);
        }

        /// <summary>
        /// Gets a new, unique reliable event identifier to use in reliable event constructor.
        /// </summary>
        /// <returns>Unique identifier for this instance of server.</returns>
        public int GetNewReliableEventId() {
            return m_lastReliableEventId++;
        }
    }

    /// <summary>
    /// A class that provides high-level client behaviors.
    /// </summary>
    public class ClientManager
    {
        NetClient m_client;

        private int m_lastReliableEventId = 0;

        /// <summary>
        /// Client's connection identifier provided by the server and stored in NetClient.
        /// </summary>
        /// <value>The connection identifier. When not connected, it returns -1</value>
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

        /// <summary>
        /// Responds to a server's reliable event.
        /// </summary>
        /// <param name="reliableEventId">The reliable event identifier.</param>
        public void RespondToReliableEvent(int reliableEventId)
        {
            ReliableEventResponse rer = new ReliableEventResponse();
            rer.m_reliableEventId = reliableEventId;
            Send(rer, false);
        }

        /// <summary>
        /// Sends an event to a server.
        /// </summary>
        /// <param name="e">Event to send.</param>
        /// <param name="reliable">Is it reliable event</param>
        /// <param name="internalResend">If <c>false</c> this event will be added to reliable events queue as a new event</param>
        /// <returns><c>true</c> if the target client is connected</returns>
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

        /// <summary>
        /// Sends raw bytestream to server
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Send(ByteStreamWriter stream) {
            m_client.Send(stream.GetBytes());
        }

        /// <summary>
        /// Resends the unresponded reliable events.
        /// </summary>
        public void ResendRemainingReliables() {
            foreach (var pair in m_reliablesToRepeat) {
                Send(pair.Value, true, true);
            }
        }

        /// <summary>
        /// Removes unreliable event from unconfirmed reliables queue.
        /// </summary>
        /// <param name="id">The event's identifier.</param>
        public void ReleaseReliable(int id)
        {
            m_reliablesToRepeat.Remove(id);
        }

        /// <summary>
        /// Gets a new, unique reliable event identifier to use in reliable event constructor.
        /// </summary>
        /// <returns>Unique identifier for this instance of client.</returns>
        public int GetNewReliableEventId() {
            return m_lastReliableEventId++;
        }

        public void Shutdown() {
            ByteStreamWriter writer = new ByteStreamWriter();
            writer.WriteByte((byte)MsgFlags.ConnectionRequest);
            writer.WriteByte((byte)HandshakeMessage.Disconnect);
            writer.WriteInteger(ConnectionId);

            for (int i = 0; i < 5; ++i) {
                 Send(writer);
            }

            m_client.Shutdown();
        }
    }

    static Network m_network;

    MessageDeserializer m_deserializer;
    MessageDispatcher m_dispatcher;
    ServerManager m_server;
    ClientManager m_client;

    /// <summary>
    /// Gets the server if the Network was initialized as server.
    /// </summary>
    /// <value>The server.</value>
    public static ServerManager Server { get { return m_network.m_server; } }
    /// <summary>
    /// Gets the client if the Network was initialized as client.
    /// </summary>
    /// <value>The client.</value>
    public static ClientManager Client { get { return m_network.m_client; } }

    bool m_isServer;

    /// <summary>
    /// A log to console method that can be colled throught the entire project. 
    /// It can be e.g. System.Console.WriteLine(...) or UnityEngine.Debug.Log(...)
    /// </summary>
    public static System.Action<object> Log;

    static Network()
    {
        m_network = new Network();
    }

    /// <summary>
    /// Initializes network with some default values
    /// </summary>
    /// <param name="isServer">if set to <c>true</c> it is initialized as server. Otherwise, it's a client.</param>
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
        if (Log == null)
            Log = (object o) => { System.Console.WriteLine(o.ToString()); };

        m_network.m_isServer = isServer;

        m_network.m_dispatcher = new MessageDispatcher();
        m_network.m_deserializer = new MessageDeserializer();
    }

    /// <summary>
    /// Initializes as server.
    /// </summary>
    /// <param name="port">The server's sending port. Server's receiving port is set to port+1.</param>
    public static void InitAsServer(int port) {
        InitBasic(true);
        NetServer server = new NetServer();
        m_network.m_deserializer.Init(server, m_network.m_dispatcher);

        server.SetDeserializer(m_network.m_deserializer);
        server.Start(port);

        ServerManager manager = new ServerManager(server);
        m_network.m_server = manager;
    }

    /// <summary>
    /// Initializes as client.
    /// </summary>
    /// <param name="serverIp">The server ip.</param>
    /// <param name="listenPort">The client's listening port</param>
    /// <param name="receivePort">The server's listening port (you will send data here)</param>
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