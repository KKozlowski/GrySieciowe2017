using System;
using System.Collections.Generic;
using UnityEngine;

public class InputEvent : EventBase
{
    public Vector2 m_direction = Vector2.zero;
    public int m_sessionId = -1;

    public override void Serialize( ByteStreamWriter writer )
    {
        writer.WriteVector2( m_direction );
        writer.WriteInteger( m_sessionId );
    }

    public override void Deserialize( ByteStreamReader reader )
    {
        m_direction = reader.ReadVector2();
        m_sessionId = reader.ReadInt();
    }

    public override byte GetId()
    {
        return InputEvent.GetStaticId();
    }

    public static byte GetStaticId()
    {
        return ( byte )EventType.Input;
    }
}