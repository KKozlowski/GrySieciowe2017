using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Net;

public class NetworkListener
{
    Socket m_socket;
    Socket m_remote;
    Thread m_thread;

    IPAddress m_address; // end point address
    int m_port;

    bool m_initialized = false;

    ~NetworkListener()
    {
        Shutdown();
    }

    public void StartListening( int port, Action<byte[], int> dataCallback )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );

        m_initialized = true;

        m_port = port;

        m_thread = new Thread( () => ListeningProc( dataCallback ) );
        m_thread.Start();
    }

    void ListeningProc( Action<byte[], int> dataCallback )
    {
        m_socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        m_socket.Bind( new IPEndPoint( IPAddress.Parse( "127.0.0.1" ), m_port ) );

        m_socket.ReceiveBufferSize = 512;
        m_socket.Listen( 10 );
        m_remote = m_socket.Accept();

        // HACK!
        m_address = ( ( IPEndPoint )m_remote.LocalEndPoint ).Address;

        m_thread = new Thread( () => ReceiveProc( dataCallback ) );
        m_thread.Start();
    }

    void ReceiveProc( Action<byte[], int> dataCallback )
    {
        m_remote.ReceiveBufferSize = 512;

        while ( m_remote.Connected )
        {
            byte[] buffer = new byte[ 512 ];
            int size = m_remote.Receive( buffer );

            if ( size > 0 )
            {
                dataCallback.Invoke( buffer, size );
            }
        }
    }

    public IPEndPoint RemoteEndPoint()
    {
        return ( IPEndPoint )m_remote.LocalEndPoint;
    }

    public bool Connected()
    {
        if ( m_remote == null || m_socket == null )
        {
            return false;
        }

        return m_remote.Connected;
    }

    public void Shutdown()
    {
        if (m_thread != null)
        {
            m_thread.Interrupt();
        }

        if (m_remote != null)
        {
            m_remote.Shutdown( SocketShutdown.Both );
        }

        if (m_socket != null)
        {
            m_socket.Shutdown( SocketShutdown.Both );
        }
    }
}

public class Connection
{
    Socket m_socket;
    Thread m_thread;
    IPAddress m_address; // end point address

    int m_port;
    bool m_initialized = false;

    ~Connection()
    {
        Shutdown();
    }

    public void Connect( string ip, int port )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );
        m_initialized = true;
        m_port = port;
        m_address = IPAddress.Parse( ip );

        m_socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        m_thread = new Thread( ConnectProc );
        m_thread.Start();
    }

    public bool Connected()
    {
        if ( m_socket == null )
            return false;

        return m_socket.Connected;
    }

    void ConnectProc()
    {
        m_socket.Connect( m_address, m_port );
    }

    public void Close()
    {
        if ( m_socket != null )
        {
            m_socket.Shutdown( SocketShutdown.Both );
            m_socket.Close();
        }

        if ( m_thread != null )
        {
            m_thread.Interrupt();
        }
    }

    public void Send( byte[] data )
    {
        m_socket.Send( data );
    }

    public int GetPort()
    {
        return m_port;
    }

    public void Shutdown()
    {
        if (m_thread != null)
        {
            m_thread.Interrupt();
        }

        if (m_socket != null)
        {
            m_socket.Shutdown(SocketShutdown.Both);
        }
    }
}

public class Client
{
    NetworkListener m_listener;
    Connection m_sender;

    public void InitConnection( string ip, int receivePort )
    {
        m_sender = new Connection();
        m_listener = new NetworkListener();

        m_listener.StartListening( receivePort, OnData );
        m_sender.Connect( ip, receivePort + 1 );

        while ( m_listener.Connected() == false )
        {
            Thread.Sleep( 10 );
        }
        System.Diagnostics.Debug.Assert( m_sender.Connected() );
    }

    void OnData( byte[] data, int size )
    {
        int result = BitConverter.ToInt32( data, 0 );
    }
}

public class Server
{
    Connection m_clientConnection;
    NetworkListener m_currentListener;
    Thread m_waitForConnection;

    public void Start( int port )
    {
        m_currentListener = new NetworkListener();
        m_currentListener.StartListening( port + 1, OnData );

        m_waitForConnection = new Thread( WaitForConnectionProc );
        m_waitForConnection.Start();
    }

    void WaitForConnectionProc()
    {
        while ( m_currentListener.Connected() == false )
        {
            Thread.Sleep( 10 );
        }

        m_clientConnection = new Connection();
        IPEndPoint endPoint = m_currentListener.RemoteEndPoint();
        m_clientConnection.Connect( endPoint.Address.ToString(), endPoint.Port - 1 );

        while ( m_clientConnection.Connected() == false )
        {
            Thread.Sleep( 10 );
        }

        m_clientConnection.Send( BitConverter.GetBytes( (int) 32 ) );
    }

    void OnData( byte[] data, int size )
    {
    }
}