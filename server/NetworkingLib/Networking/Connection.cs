using System.Net.Sockets;
using System.Net;

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