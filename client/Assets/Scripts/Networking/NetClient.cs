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

public class NetClient
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