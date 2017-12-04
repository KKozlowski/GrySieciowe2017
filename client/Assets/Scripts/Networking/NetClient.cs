#define LOG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Net;

/// <summary>
/// Contains traditional SYN, SYNACK and ACK messages for Three-Way Handshake, and a Disconnect message.
/// </summary>
public enum HandshakeMessage : byte {
    SYN,
    SYNACK,
    ACK,
    /// <summary>
    /// Sent when client disconnects from server.
    /// </summary>
    Disconnect
}

/// <summary>
/// Interface for object that can perform Three-way handshake.
/// </summary>
public interface IHanshakable {
    /// <summary>
    /// Handles data that are sent by server and client during Three-Way Hanshake. They are not Events.
    /// </summary>
    /// <param name="type">Type of the hanshake message.</param>
    /// <param name="stream">Byte data that was received with the message.</param>
    /// <param name="endpoint">Source of the message.</param>
    void HandleConnectionData(HandshakeMessage type, ByteStreamReader stream, IPEndPoint endpoint);
}

/// <summary>
/// The class responsible for client behaviors: connecting to server, sending packages etc.
/// </summary>
public class NetClient : IHanshakable
{
    /// <summary>
    /// All the states NetClient can have.
    /// </summary>
    public enum ConnectionState {
        /// <summary>
        /// NetClient isn't connected and doesn't do anything.
        /// </summary>
        Idle,
        /// <summary>
        /// NetClient isn't connected but it is during Three-Way handshake
        /// </summary>
        Connecting,
        /// <summary>
        /// Net client received its ConnectionID from server and completed its Three-Way handshake.
        /// </summary>
        Connected
    }

    /// <summary>
    /// Receives data from server and calls OnData.
    /// </summary>
    Listener m_listener;
    /// <summary>
    /// Sends data to server.
    /// </summary>
    Connection m_sender;

    /// <summary>
    /// Interprets received bytes.
    /// </summary>
    private MessageDeserializer deserializer;

    public ConnectionState State { get; private set; }

    /// <summary>
    /// Gets the connection identifier received from the server during Handshake.
    /// </summary>
    /// <value>Connection identifier. The default value is -1.</value>
    public int ConnectionId { get; private set; }

    public NetClient() {
        ConnectionId = -1;
    }

    /// <summary>
    /// Connects client to a specified server.
    /// </summary>
    /// <param name="ip">Server's ip.</param>
    /// <param name="listenPort">Client's port (port the server will send date to).</param>
    /// <param name="receivePort">Server's port (port the client will send data to).</param>
    public void Connect( string ip, int listenPort, int receivePort)
    {
        m_listener = new Listener();
        m_listener.Init(listenPort, false);

        m_sender = new Connection();
        m_sender.Connect( ip, receivePort + 1 );

        m_listener.SetDataCallback(OnData);

        HandshakeStepOne(listenPort);
    }

    /// <summary>
    /// Shuts down the connection with server.
    /// </summary>
    public void Shutdown()
    {
        if ( m_sender != null )
            m_sender.Shutdown();

        if ( m_listener != null )
            m_listener.Shutdown();

        State = ConnectionState.Idle;
        ConnectionId = -1;
    }

    /// <summary>
    /// Handles received data.
    /// </summary>
    /// <param name="data">Bytes with data.</param>
    /// <param name="endpoint">Data source.</param>
    void OnData(byte[] data, IPEndPoint endpoint) {
        //Network.Log("Data received");
        deserializer.HandleData(data, endpoint);
    }

    /// <summary>
    /// Injects deserializer instance.
    /// </summary>
    /// <param name="md">Specific deserializer</param>
    public void SetDeserializer(MessageDeserializer md) {
        deserializer = md;
    }

    /// <summary>
    /// Sends raw data to server.
    /// </summary>
    /// <param name="data">The data.</param>
    public void Send( byte[] data )
    {
        m_sender.Send( data );
    }

    /// <summary>
    /// Starts handshake process.
    /// </summary>
    /// <param name="receivePort">The port server should send data to.</param>
    public void HandshakeStepOne(int receivePort) {
        ByteStreamWriter writer = new ByteStreamWriter();
        writer.WriteByte((byte)MsgFlags.ConnectionRequest);
        writer.WriteByte((byte)HandshakeMessage.SYN);
        writer.WriteInteger(receivePort);
        m_sender.Send(writer.GetBytes());
        State = ConnectionState.Connecting;
    }

    /// <summary>
    /// Sends a confirmation of receiving the Connection ID to server (finalizes the connection on the Client side)
    /// </summary>
    /// <param name="connectionId">The connection identifier.</param>
    public void HandshakeStepThree(int connectionId) {
        ByteStreamWriter writer = new ByteStreamWriter();
        writer.WriteByte((byte)MsgFlags.ConnectionRequest);
        writer.WriteByte((byte)HandshakeMessage.ACK);
        writer.WriteInteger(connectionId);

        for (int i = 0; i < 5; ++i) {
            m_sender.Send(writer.GetBytes());
        }
        
        ConnectionId = connectionId;
        State = ConnectionState.Connected;

        Network.Log(State + ", " + ConnectionId);
    }

    public void HandleConnectionData(HandshakeMessage type, ByteStreamReader stream, IPEndPoint endpoint) {
        Console.WriteLine(type);
        if (type == HandshakeMessage.SYNACK && State != ConnectionState.Connected) {
            int connectionId = stream.ReadInt();
            HandshakeStepThree(connectionId);
        }
    }
}