#define LOG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Net;

public class Listener
{
    Socket m_socket;
    Thread m_receivingThread;

    bool m_initialized = false;

    ~Listener()
    {
        Shutdown();
    }

    public void Init( int port )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );

        m_socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
        m_socket.Bind( new IPEndPoint( IPAddress.Parse( "127.0.0.1" ), port ) );

        m_initialized = true;

        m_receivingThread = new Thread( ReceiveProc );
        m_receivingThread.Start();
    }

    public void Shutdown()
    {
        if ( m_receivingThread != null )
        {
            m_receivingThread.Interrupt();
        }

        if ( m_socket != null && m_socket.Connected )
        {
            m_socket.Shutdown( SocketShutdown.Both );
        }

#if LOG
        Net.Dbg.Log( "Network listener shutdown" );
#endif
    }

    void ReceiveProc()
    {
        while ( true )
        {
            byte[] data = new byte[ 512 ];
            int size = m_socket.Receive( data );

            if ( size > 0 )
            {
                OnData( data );
            }
        }
    }

    void OnData( byte[] data )
    {

    }
}

public class Connection
{
    Socket m_socket;

    bool m_initialized = false;

    ~Connection()
    {
        Shutdown();
    }

    public void Connect( string ip, int port )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );

        m_socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );

        IPAddress address = IPAddress.Parse( ip );
        m_socket.Connect( address, port );

        m_initialized = true;
    }

    public void InitListener( Socket listenerSocket )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );
        m_initialized = true;

        m_socket = listenerSocket;
    }

    public bool Connected()
    {
        if ( m_socket == null )
            return false;

        return m_socket.Connected;
    }

    void ConnectProc( IPAddress addr, int port )
    {
        m_socket.Connect( addr, port );
    }

    public void Send( byte[] data )
    {
        m_socket.Send( data );
    }

    public void Shutdown()
    {
        if ( m_socket != null && m_socket.Connected )
        {
            m_socket.Shutdown( SocketShutdown.Both );
        }

#if LOG
        Net.Dbg.Log( "Connection shutdown" );
#endif
    }
}

public class Client
{
    Listener m_listener;
    Connection m_sender;

    public void Connect( string ip, int receivePort )
    {
        m_listener = new Listener();
        m_listener.Init( receivePort );

        m_sender = new Connection();
        m_sender.Connect( ip, receivePort + 1 );

        byte[] msg = BitConverter.GetBytes( 2017 );
        m_sender.Send( msg );
    }

    void OnData( byte[] data, int size )
    {
        int result = BitConverter.ToInt32( data, 0 );
    }

    public void Shutdown()
    {
        if ( m_sender != null )
            m_sender.Shutdown();

        if ( m_listener != null )
            m_listener.Shutdown();
    }
}

public class Server
{
    class ConnectionEntity
    {
        public Connection m_sender;
        public Connection m_receiver;
    }

    List< ConnectionEntity > m_entities = new List<ConnectionEntity>();

    Listener m_listener;

    int m_sendingPort;
    int m_receivePort;

    public void Start( int sendingPort )
    {
        m_sendingPort = sendingPort;
        m_receivePort = sendingPort + 1;

        m_listener = new Listener();
        m_listener.Init( m_receivePort );
    }
}