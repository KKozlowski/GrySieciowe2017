#define LOG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Net;

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