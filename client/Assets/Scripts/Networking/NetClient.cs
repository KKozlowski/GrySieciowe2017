#define LOG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Net;

public class ConnectionListener
{
    Socket m_socket;
    Thread m_thread;

    IPAddress m_address; // end point address
    int m_port;

    bool m_initialized = false;

    ~ConnectionListener()
    {
        Shutdown();
    }

    public void Init( int port )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );

        m_socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        m_socket.Bind( new IPEndPoint( IPAddress.Parse( "127.0.0.1" ), m_port ) );

        m_initialized = true;
    }

    public void StartListening( Action<Socket> acceptedCallback )
    {
        m_thread = new Thread( () => ListeningProc( acceptedCallback ) );
        m_thread.Start();
    }

    void ListeningProc( Action<Socket> acceptedCallback )
    {
        m_socket.Listen( 10 );
        Socket remote = m_socket.Accept();

#if LOG
        Net.Dbg.Log( "NetworkListener: Accepted socket: " + ( ( IPEndPoint )remote.LocalEndPoint ).ToString() );
#endif

        acceptedCallback( remote );
    }

    public void Shutdown()
    {
        if ( m_thread != null )
        {
            m_thread.Interrupt();
        }

        if ( m_socket != null && m_socket.Connected )
        {
            m_socket.Shutdown( SocketShutdown.Both );
        }

#if LOG
        Net.Dbg.Log( "Network listener shutdown" );
#endif
    }
}

public class Connection
{
    Socket m_socket;
    Thread m_thread;

    bool m_initialized = false;

    ~Connection()
    {
        Shutdown();
    }

    public void Connect( string ip, int port )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );

        IPAddress address = IPAddress.Parse( ip );

        m_socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        m_thread = new Thread( () => ConnectProc( address, port ) );
        m_thread.Start();

        m_initialized = true;
    }

    public void InitListener( Socket listenerSocket )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );
        m_initialized = true;

        m_socket = listenerSocket;
        m_thread = new Thread( ReceiveProc );
        m_thread.Start();
    }

    void ReceiveProc()
    {
        while ( m_socket.Connected )
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
        if ( m_thread != null )
        {
            m_thread.Interrupt();
        }

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
    ConnectionListener m_listener;
    Connection m_sender;
    Connection m_receiver;

    public void Connect( string ip, int receivePort )
    {
        m_listener = new ConnectionListener();
        m_listener.Init( receivePort );
        m_listener.StartListening( OnAccepted );

        m_sender = new Connection();
        m_sender.Connect( ip, receivePort + 1 );
    }

    void OnAccepted( Socket remoteSender )
    {
        m_receiver = new Connection();
        m_receiver.InitListener( remoteSender );
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

    ConnectionListener m_connector;

    int m_sendingPort;
    int m_receivePort;

    public void Start( int sendingPort )
    {
        m_sendingPort = sendingPort;
        m_receivePort = sendingPort + 1;

        m_connector = new ConnectionListener();
        m_connector.Init( m_receivePort );
        m_connector.StartListening( OnAccepted );
    }

    void OnAccepted( Socket remote )
    {
        Connection receiver = new Connection();
        receiver.InitListener( remote );

        Connection sender = new Connection();
        IPEndPoint endPoint = (IPEndPoint)remote.LocalEndPoint;
        sender.Connect( endPoint.Address.ToString(), m_sendingPort );

        ConnectionEntity newEntity = new ConnectionEntity()
        {
            m_receiver = receiver,
            m_sender = sender
        };

        m_entities.Add( newEntity );
    }
}