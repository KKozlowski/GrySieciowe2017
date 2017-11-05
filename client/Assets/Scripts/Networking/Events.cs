using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public enum EventType : byte
{
    HelloWorld = 1,
    Input = 2
}

public abstract class EventBase
{
    public abstract void Serialize( ByteStreamWriter writer );
    public abstract void Deserialize( ByteStreamReader reader );
    public abstract byte GetId();

    public EventType GetEventType()
    {
        return (EventType)GetId();
    }
}

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
        return (byte)EventType.HelloWorld;
    }

    public static byte GetStaticId()
    {
        return ( byte )EventType.HelloWorld;
    }
}

public class EventsFactory
{
    FastMap<Type> m_events = new FastMap<Type>();

    public EventsFactory()
    {
        Init();
    }

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
