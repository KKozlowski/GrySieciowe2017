using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

public class Listener
{
    UdpClient m_udp;
    Thread m_receivingThread;
    int m_port;
    Action< byte[] > m_dataCallback;

    bool m_initialized = false;

    ~Listener()
    {
        Shutdown();
    }

    public void Init( int port )
    {
        System.Diagnostics.Debug.Assert( !m_initialized, "Already initialized" );

        m_udp = new UdpClient( port );
        m_initialized = true;
        m_port = port;

        m_receivingThread = new Thread( ReceiveProc );
        m_receivingThread.Start();
    }

    public void Shutdown()
    {
        if ( m_receivingThread != null )
        {
            m_receivingThread.Interrupt();
        }

        if ( m_udp != null )
        {
            m_udp.Close();
        }

#if LOG
        Net.Dbg.Log( "Network listener shutdown" );
#endif
    }

    public void SetDataCallback( Action<byte[]> onData )
    {
        m_dataCallback = onData;
    }

    void ReceiveProc()
    {
        IPEndPoint endpoint = new IPEndPoint( IPAddress.Any, m_port );
        while ( true )
        {
            byte[] data = m_udp.Receive( ref endpoint );

            if ( data.Length > 0 )
            {
                if ( m_dataCallback != null )
                    m_dataCallback.Invoke( data );
            }
        }
    }
}
