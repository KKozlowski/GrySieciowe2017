using System.Net.Sockets;
using System.Net;

/// <summary>
/// Sends data to the ip and port given during initialization.
/// </summary>
public class Connection {
    UdpClient m_udp;
    bool m_initialized = false;

    public IPAddress address { get; private set; }
    public int port { get; private set; }

    ~Connection()
    {
        Shutdown();
    }

    public void Connect(IPEndPoint endpoint) {
        System.Diagnostics.Debug.Assert(!m_initialized, "Already initialized");

        address = endpoint.Address;
        port = endpoint.Port;
        m_udp = new UdpClient();
        m_udp.Connect(endpoint);
        m_initialized = true;
    }

    public void Connect( string ip, int port )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );

        address = IPAddress.Parse( ip );
        this.port = port;
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