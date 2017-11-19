#define LOG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Net;

public enum HandshakeMessage : byte {
    SYN,
    SYNACK,
    ACK
}

public interface IHanshakable {
    void HandleConnectionData(HandshakeMessage type, ByteStreamReader stream, IPEndPoint endpoint);
}

public class NetClient : IHanshakable
{
    public enum ConnectionState {
        Idle,
        Connecting,
        Connected
    }

    Listener m_listener;
    Connection m_sender;

    private MessageDeserializer deserializer;

    public ConnectionState State { get; private set; }
    public int ConnectionId { get; private set; }

    public NetClient() {
        ConnectionId = -1;
    }

    public void Connect( string ip, int listenPort, int receivePort )
    {
        m_listener = new Listener();
        m_listener.Init(listenPort);

        m_sender = new Connection();
        m_sender.Connect( ip, receivePort + 1 );

        m_listener.SetDataCallback(OnData);

        HandshakeStepOne(listenPort);
    }

    public void Shutdown()
    {
        if ( m_sender != null )
            m_sender.Shutdown();

        if ( m_listener != null )
            m_listener.Shutdown();
    }

    void OnData(byte[] data, IPEndPoint endpoint) {
        Console.WriteLine("Data received");
        deserializer.HandleData(data, endpoint);
    }

    public void SetDeserializer(MessageDeserializer md) {
        deserializer = md;
    }

    public void Send( byte[] data )
    {
        m_sender.Send( data );
    }

    public void HandshakeStepOne(int receivePort) {
        ByteStreamWriter writer = new ByteStreamWriter();
        writer.WriteByte((byte)MsgFlags.ConnectionRequest);
        writer.WriteByte((byte)HandshakeMessage.SYN);
        writer.WriteInteger(receivePort);
        m_sender.Send(writer.GetBytes());
        State = ConnectionState.Connecting;
    }

    public void HandshakeStepThree(int connectionId) {
        ByteStreamWriter writer = new ByteStreamWriter();
        writer.WriteByte((byte)MsgFlags.ConnectionRequest);
        writer.WriteByte((byte)HandshakeMessage.ACK);
        writer.WriteInteger(connectionId);
        m_sender.Send(writer.GetBytes());
        ConnectionId = connectionId;
        State = ConnectionState.Connected;

        Console.WriteLine(State + ", " + ConnectionId);
    }

    public void HandleConnectionData(HandshakeMessage type, ByteStreamReader stream, IPEndPoint endpoint) {
        Console.WriteLine(type);
        if (type == HandshakeMessage.SYNACK && State != ConnectionState.Connected) {
            int connectionId = stream.ReadInt();
            HandshakeStepThree(connectionId);
        }
    }
}

public class UnreliableEventSender
{
    IdAllocator m_eventsId = new IdAllocator();

    class EventSending
    {
        public int m_connectionId;
        public byte[] m_data;
        public int m_eventId = -1;
        public int m_attempts = 0;
    }
}