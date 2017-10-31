using System.Net.Sockets;
using System.Net;

public class Connection
{
    UdpClient m_udp;
    bool m_initialized = false;

    ~Connection()
    {
        Shutdown();
    }

    public void Connect( string ip, int port )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );

        IPAddress address = IPAddress.Parse( ip );
        IPEndPoint endpoint = new IPEndPoint( address, port );
        m_udp = new UdpClient();
        m_udp.Connect( endpoint );
        m_initialized = true;
    }

    public void Send( byte[] data )
    {
        m_udp.Send( data, data.Length );
    }

    public void Shutdown()
    {
        m_udp.Close();

#if LOG
        Net.Dbg.Log( "Connection shutdown" );
#endif
    }
}