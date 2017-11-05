using System;
using System.Collections.Generic;

public interface IEventListener
{
    EventType GetEventType();
    bool Execute( EventBase e );
}

public class MessageDispatcher
{
    List< IEventListener > m_listeners = new List<IEventListener>();

    public void PushEvent( EventBase evnt )
    {
        for ( int i = 0; i < m_listeners.Count; ++i )
        {
            if ( m_listeners[i].GetEventType() == evnt.GetEventType() )
            {
                if ( m_listeners[i].Execute( evnt ) )
                    return;
            }
        }
    }

    public void AddListener( IEventListener listener )
    {
        m_listeners.Add( listener );
    }

    public void RemoveListener( IEventListener listener )
    {
        m_listeners.Remove( listener );
    }
}