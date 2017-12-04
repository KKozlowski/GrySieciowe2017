using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// Listens for data from given ip and port, then sends them through callback. Based on UDP.
/// </summary>
public class Listener
{
    UdpClient m_udp;
    Thread m_receivingThread;
    int m_port;

    /// <summary>
    /// Methods that is called when new data is received.
    /// </summary>
    Action< byte[], IPEndPoint> m_dataCallback;

    bool m_initialized = false;

    ~Listener()
    {
        Network.Log("Listener destroyed");
        Shutdown();
    }

    private void BasicInit( int port ) {
        System.Diagnostics.Debug.Assert(!m_initialized, "Already initialized");

        m_udp = new UdpClient(port);
        m_initialized = true;
        m_port = port;
    }

    /// <summary>
    /// Starts listening using thread or recursive method.
    /// </summary>
    /// <param name="port">Port to listen.</param>
    /// <param name="withThread">if set to <c>true</c>, starts listning using a new thread. Otherwise it uses UdpClient.BeginListening(...) method.</param>
    public void Init( int port, bool withThread = true )
    {
        BasicInit(port);

        if (withThread) {
            m_receivingThread = new Thread(ReceiveProc);
            m_receivingThread.Start();
        } else {
            m_udp.BeginReceive(new AsyncCallback(ReceiveRecur), null);
        }
        
    }

    public void Shutdown()
    {
        if ( m_receivingThread != null )
        {
            m_receivingThread.Abort();
        }

        if ( m_udp != null )
        {
            m_udp.Close();
        }

#if LOG
        Net.Dbg.Log( "Network listener shutdown" );
#endif
    }

    public void SetDataCallback( Action<byte[], IPEndPoint> onData )
    {
        m_dataCallback = onData;
    }

    /// <summary>
    /// Listening thread logic.
    /// </summary>
    void ReceiveProc()
    {
        IPEndPoint endpoint = new IPEndPoint( IPAddress.Any, m_port );
        while ( true )
        {
            byte[] data = m_udp.Receive( ref endpoint );

            if ( data.Length > 0 )
            {
                if ( m_dataCallback != null )
                    m_dataCallback.Invoke( data, endpoint );
            }
        }
    }

    /// <summary>
    /// Listening logic based on UdpClient.BeginReceive(...). Can be threadless.
    /// </summary>
    /// <param name="res">The previous receiving result.</param>
    private void ReceiveRecur(IAsyncResult res) {
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, m_port);
        byte[] data = m_udp.EndReceive(res, ref endpoint);

        //Process codes
        //Network.Log("RECEIVED " + data.Length + " bytes {" + data.StructuralToString()+"}");
        if (data.Length > 0) {
            if (m_dataCallback != null) {
                m_dataCallback.Invoke(data, endpoint);
            }
                
        }
        m_udp.BeginReceive(new AsyncCallback(ReceiveRecur), null);
    }
}
