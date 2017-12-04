using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// All types the event can have
/// </summary>
public enum EventType : byte
{
    /// <summary>
    /// Unusable empty event (e.g. base class)
    /// </summary>
    Abstract = 0,

    /// <summary>
    /// Test event
    /// </summary>
    HelloWorld = 1,

    /// <summary>
    /// Player's input
    /// </summary>
    Input = 2,

    /// <summary>
    /// A spawn request, sent by Client after connection to call a spawn, and sent by Server to a Client to inform about respawn.
    /// </summary>
    SpawnRequest = 3,

    /// <summary>
    /// All connected players data packaged into one stream.
    /// </summary>
    PlayerState = 4,

    /// <summary>
    /// When a player performs a shot.
    /// </summary>
    Shot = 5,

    /// <summary>
    /// Response to a reliable event, that stops the sender from spamming it further.
    /// </summary>
    ReliableEventResponse = 6
}

/// <summary>
/// Base abstract class that all the events inherit from.
/// </summary>
public abstract class EventBase
{
    /// <summary>
    /// Save event's data into a bytestream.
    /// </summary>
    /// <param name="writer">The writer.</param>
    public abstract void Serialize( ByteStreamWriter writer );
    /// <summary>
    /// Loads event's data from a bytestream.
    /// </summary>
    /// <param name="reader">The reader.</param>
    public abstract void Deserialize( ByteStreamReader reader );

    /// <summary>
    /// Returns event type turned into a byte.
    /// </summary>
    /// <returns>Event type turned into a byte.</returns>
    public abstract byte GetId();

    /// <summary>
    /// Gets this event's type enum.
    /// </summary>
    /// <returns>EventType.</returns>
    public EventType GetEventType()
    {
        return (EventType)GetId();
    }
}

/// <summary>
/// Base class for all the reliable events. Reliable events are constantly sent a sender untill it gers a confirmation (Reliable event response)
/// </summary>
public abstract class ReliableEventBase : EventBase {
    /// <summary>
    /// This event's unique identifier granted by a counter in server or client.
    /// </summary>
    public int m_reliableEventId;
    public override void Serialize(ByteStreamWriter writer) {
        writer.WriteInteger(m_reliableEventId);
    }

    public override void Deserialize(ByteStreamReader reader) {
        m_reliableEventId = reader.ReadInt();
    }

    public static byte GetStaticId() {
        return (byte)EventType.Abstract;
    }
}

/// <summary>
/// Empty "Hello world" test event.
/// </summary>
public class TestEvent : EventBase
{
    public override void Serialize( ByteStreamWriter writer )
    {
    }

    public override void Deserialize( ByteStreamReader reader )
    {
    }

    public override byte GetId()
    {
        return GetStaticId();
    }

    public static byte GetStaticId()
    {
        return ( byte )EventType.HelloWorld;
    }
}

/// <summary>
/// A class that constructs event objects based on type identifier.
/// </summary>
public class EventsFactory
{
    FastMap<Type> m_events = new FastMap<Type>();

    public EventsFactory()
    {
        Init();
    }

    /// <summary>
    /// Through reflection, if finds and stores all the event classes in project.
    /// </summary>
    void Init()
    {
        List<Type> eventClasses = GetEventClasses();

        int count = eventClasses.Count;

        MethodInfo method;
        for ( int i = 0; i < count; ++i )
        {
            method = eventClasses[i].GetMethod( "GetStaticId", BindingFlags.Public | BindingFlags.Static );
            System.Diagnostics.Debug.Assert( method != null );
            System.Diagnostics.Debug.Assert( method.ReturnType == typeof( byte ) );

            byte id = (byte)method.Invoke( eventClasses[i], null );
            m_events.AddUnique( id, eventClasses[i] );
        }
        MethodInfo methodInfo = eventClasses[0].GetMethod( "GetID", BindingFlags.Static );
    }

    /// <summary>
    /// Gets the event classes.
    /// </summary>
    /// <returns>List of all the indexed event types.</returns>
    List< Type > GetEventClasses()
    {
        List< Type > result = new List<Type>();

        Type baseType = typeof( EventBase );
        Assembly assembly = baseType.Assembly;

        Type[] allTypes = assembly.GetTypes();
        for ( int i = 0; i < allTypes.Length; ++i )
        {
            if ( allTypes[i].IsSubclassOf( baseType ) )
            {
                result.Add( allTypes[i] );
            }
        }

        return result;
    }

    /// <summary>
    /// Creates an event object based on a given type specifier.
    /// </summary>
    /// <param name="id">Type specifier.</param>
    /// <returns>A specialized event object.</returns>
    public EventBase CreateEvent( int id )
    {
        EventBase result;
        Type eventType = m_events.Find( id );
        System.Diagnostics.Debug.Assert( eventType != null, "Event type not found id: " + id );
        System.Diagnostics.Debug.Assert( eventType.IsSubclassOf( typeof( EventBase ) ), "Event doesn't inherit from EventBase id: " + id );

        result = (EventBase)Activator.CreateInstance( eventType );
        return result;
    }
}
