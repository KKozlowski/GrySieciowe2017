using System.Collections.Generic;
using System.Net;
using System;
using System.Linq;

/// <summary>
/// The class responsible for server behaviors: storing all the client connections and sending data to the clients.
/// </summary>
public class NetServer : IHanshakable {
    /// <summary>
    /// Basic class for the key connection data.
    /// </summary>
    abstract class BaseConnectionEntity {
        /// <summary>
        /// Can send data to the corresponding client.
        /// </summary>
        public Connection m_sender;
        /// <summary>
        /// The connection identifier for corresponding client
        /// </summary>
        public int m_connectionId;
    }

    /// <summary>
    /// Data of a client who completed handshake step three and thus is successfully connected.
    /// </summary>
    class ConnectionEntity : BaseConnectionEntity
    {
        public ConnectionEntity(PendingConnectionEntity pending) {
            m_sender = pending.m_sender;
            m_connectionId = pending.m_connectionId;
        }
    }

    /// <summary>
    /// Data of a client who completed handshake step one.
    /// </summary>
    class PendingConnectionEntity : BaseConnectionEntity {
        public DateTime m_pendingStartTime;
    }

    IdAllocator m_connectionId = new IdAllocator();
    Dictionary<int, ConnectionEntity > m_entities = new Dictionary<int, ConnectionEntity>();

    List<PendingConnectionEntity> m_pending = new List<PendingConnectionEntity>();
    List<int> m_toDisconnect = new List<int>();

    /// <summary>
    /// Called when player is disconnected from server. It passes connectionId and [if was disconnected by afk] bool.
    /// </summary>
    public System.Action<int, bool> OnDisconnect;

    Listener m_listener;

    int m_sendingPort;
    int m_receivePort;

    /// <summary>
    /// Time after which server stops performing handshake step two on pending connection.
    /// </summary>
    float timeToStopTryingWithPending = 10f;
    /// <summary>
    /// Times after which pending connection is removed if it didn't react at all.
    /// </summary>
    float timeToRemovePending = 15f;

    private MessageDeserializer deserializer;

    /// <summary>
    /// Initializes server listening.
    /// </summary>
    /// <param name="sendingPort">The server's sending port. Receiving port is set to sendingPort+1</param>
    public void Start( int sendingPort )
    {
        m_sendingPort = sendingPort;
        m_receivePort = sendingPort + 1;

        m_listener = new Listener();
        m_listener.Init( m_receivePort );

        m_listener.SetDataCallback( OnData );
    }

    void OnData( byte[] data, IPEndPoint endpoint )
    {
        //Network.Log("Data received");
        deserializer.HandleData(data, endpoint);
    }

    public Connection GetConnectionById(int id) {
        ConnectionEntity ce = null;
        if (m_entities.TryGetValue(id, out ce)) {
            return ce.m_sender;
        }
        return null;
    }

    public List<int> GetAllConnectionIds()
    {
        return m_entities.Keys.ToList();
    }

    public void SetDeserializer(MessageDeserializer md) {
        deserializer = md;
    }

    private void HandshakeStepTwo(BaseConnectionEntity con) {
        ByteStreamWriter writer = new ByteStreamWriter();
        writer.WriteByte((byte)MsgFlags.ConnectionRequest);
        writer.WriteByte((byte)HandshakeMessage.SYNACK);
        writer.WriteInteger(con.m_connectionId);
        con.m_sender.Send(writer.GetBytes());
        con.m_sender.Send(writer.GetBytes());
        con.m_sender.Send(writer.GetBytes());
        con.m_sender.Send(writer.GetBytes());
    }

    public void HandleConnectionData(HandshakeMessage type, ByteStreamReader stream, IPEndPoint endpoint) {
        Console.WriteLine(type);
        if (type == HandshakeMessage.SYN) {
            PendingConnectionEntity ce = new PendingConnectionEntity();
            ce.m_connectionId = m_connectionId.Allocate();
            ce.m_sender = new Connection();
            int port = stream.ReadInt();
            ce.m_pendingStartTime = DateTime.Now;
            ce.m_sender.Connect(new IPEndPoint(endpoint.Address, port));
            m_pending.Add(ce);

            HandshakeStepTwo(ce);
        } else if (type == HandshakeMessage.ACK) {
            int id = stream.ReadInt();
            PendingConnectionEntity chosen = m_pending.FirstOrDefault(x => x.m_connectionId == id);
            if (chosen != null) {
                m_pending.Remove(chosen);
                m_entities[chosen.m_connectionId] = new ConnectionEntity(chosen);
                Network.Server.World.AddPlayer(chosen.m_connectionId);
            }
        } else if (type == HandshakeMessage.Disconnect)
        {
            int id = stream.ReadInt();
            Disconnect(id, false);
        }
    }

    /// <summary>
    /// Disconnects a given client
    /// </summary>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="afk">if set to <c>true</c>, the reason was player afk (lack of new packeges coming).</param>
    public void Disconnect(int connectionId, bool afk)
    {
        ConnectionEntity ce = null;
        m_entities.TryGetValue(connectionId, out ce);
        if (ce != null)
        {
            ce.m_sender.Shutdown();
            m_entities.Remove(connectionId);
            OnDisconnect?.Invoke(connectionId, afk);
        }
        
        
    }

    /// <summary>
    /// Loops through all the pending connections and performs handshake step two on them. If they are still idle for a long time, connections get canceled.
    /// </summary>
    public void TryConnectToAllPending() {
        List<PendingConnectionEntity> toRemove = new List<PendingConnectionEntity>();
        foreach(PendingConnectionEntity pce in m_pending) {
            float secondsOld = (DateTime.Now - pce.m_pendingStartTime).Seconds;
            if (secondsOld > timeToRemovePending) {
                toRemove.Add(pce);
            } else if (secondsOld > timeToStopTryingWithPending) {
                continue;
            } else {
                HandshakeStepTwo(pce);
            }
        }

        toRemove.ForEach(x => m_pending.Remove(x));
    }
}

