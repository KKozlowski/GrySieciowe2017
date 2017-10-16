using System.Collections.Generic;

public class NetServer
{
    class ConnectionEntity
    {
        public Connection m_sender;
        public int m_connectionId;
    }

    IdAllocator m_connectionId = new IdAllocator();
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

