using System.Collections.Generic;
using System.Net;
using System;
using System.Linq;

public class NetServer : IHanshakable {
    abstract class BaseConnectionEntity {
        public Connection m_sender;
        public int m_connectionId;
    }

    class ConnectionEntity : BaseConnectionEntity
    {
        public ConnectionEntity(PendingConnectionEntity pending) {
            m_sender = pending.m_sender;
            m_connectionId = pending.m_connectionId;
        }
    }

    class PendingConnectionEntity : BaseConnectionEntity {
        public DateTime m_pendingStartTime;
    }

    IdAllocator m_connectionId = new IdAllocator();
    Dictionary<int, ConnectionEntity > m_entities = new Dictionary<int, ConnectionEntity>();

    List<PendingConnectionEntity> m_pending = new List<PendingConnectionEntity>();
    List<int> m_toDisconnect = new List<int>();

    public System.Action<int, bool> OnDisconnect;

    Listener m_listener;

    int m_sendingPort;
    int m_receivePort;

    float timeToStopTryingWithPending = 10f;
    float timeToRemovePending = 15f;

    private MessageDeserializer deserializer;

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

