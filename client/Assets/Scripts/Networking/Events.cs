using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public enum EventType : byte
{
    HelloWorld = 1
}

public abstract class EventBase
{
    public abstract void Deserialize( ByteStreamReader reader );
    public abstract void Serialize( ByteStreamWriter writer );
}

public class TestEvent : EventBase
{
    public override void Serialize( ByteStreamWriter writer )
    {
    }

    public override void Deserialize( ByteStreamReader reader )
    {
    }
}

public class EventsFactory
{
    FastMap<Type> m_events;

    public EventsFactory()
    {
        Init();
    }

    void Init()
    {
        List<Type> eventClasses = GetEventClasses();
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
}
