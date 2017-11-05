using System;
using System.Collections.Generic;
using UnityEngine;

public class InputEvent : EventBase
{
    Vector2 m_direction = Vector2.zero;

    public override void Serialize( ByteStreamWriter writer )
    {
        writer.WriteVector2( m_direction );
    }

    public override void Deserialize( ByteStreamReader reader )
    {
        m_direction = reader.ReadVector2();
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