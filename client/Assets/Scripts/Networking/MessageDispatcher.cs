using System;
using System.Collections.Generic;

/// <summary>
/// Interface for an event listener.
/// </summary>
public interface IEventListener
{
    /// <summary>
    /// Gets the type of the events this instance handles.
    /// </summary>
    /// <returns>EventType.</returns>
    EventType GetEventType();
    /// <summary>
    /// Handles an event.
    /// </summary>
    /// <param name="e">The event.</param>
    /// <returns><c>true</c> if this is supposed to be the last handler of the event.</returns>
    bool Execute( EventBase e );
}

/// <summary>
/// Additional logic for IEventListener that listen to reliable events.
/// </summary>
public class ReliableEventListener
{
    Dictionary<int, HashSet<int>> executedEventsForEachUser = new Dictionary<int, HashSet<int>>();

    /// <summary>
    /// Marks given event (by id) from given user as executed.
    /// </summary>
    /// <param name="user">The client identifier. If it was received from server, it can be -1</param>
    /// <param name="eventId">The event identifier.</param>
    protected void AddExecuted(int user, int eventId)
    {
        HashSet<int> list = null;
        if (!executedEventsForEachUser.TryGetValue(user, out list))
        {
            list = new HashSet<int>();
            executedEventsForEachUser[user] = list;
        }
        list.Add(eventId);
    }

    /// <summary>
    /// Checks if given event (by id) from given user was executed.
    /// </summary>
    /// <param name="user">The user identifier.</param>
    /// <param name="eventId">The event identifier.</param>
    /// <returns><c>true</c> if was executed, <c>false</c> otherwise. If it was executed already, the event should be confirmed anyway, but not executed.</returns>
    public bool WasExecuted(int user, int eventId)
    {
        HashSet<int> list = null;
        if (!executedEventsForEachUser.TryGetValue(user, out list))
        {
            return false;
        }

        return list.Contains(eventId);
    }
}

/// <summary>
/// Stores event listeners and sends data for them.
/// </summary>
public class MessageDispatcher
{
    List< IEventListener > m_listeners = new List<IEventListener>();

    public void PushEvent( EventBase evnt )
    {
        for ( int i = 0; i < m_listeners.Count; ++i )
        {
            if ( m_listeners[i].GetEventType() == evnt.GetEventType() )
            {
                if (m_listeners[i].Execute(evnt)) {
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Adds a new listener to the collection
    /// </summary>
    /// <param name="listener">New listener.</param>
    public void AddListener( IEventListener listener )
    {
        m_listeners.Add( listener );
    }

    /// <summary>
    /// Removes the listener from collection.
    /// </summary>
    /// <param name="listener">The listener.</param>
    public void RemoveListener( IEventListener listener )
    {
        m_listeners.Remove( listener );
    }
}