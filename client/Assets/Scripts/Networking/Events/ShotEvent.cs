using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class ShotEvent : ReliableEventBase
{
    public int m_who;
    public Vector2 m_direction;
    public Vector2 m_point; //only when server sends

    public override void Serialize(ByteStreamWriter writer) {
        base.Serialize(writer);
        writer.WriteInteger(m_who);
        writer.WriteVector2(m_direction);
        writer.WriteVector2(m_point);
    }

    public override void Deserialize(ByteStreamReader reader)
    {
        base.Deserialize(reader);
        m_who = reader.ReadInt();
        m_direction = reader.ReadVector2();
        m_point = reader.ReadVector2();
    }

    public override byte GetId() {
        return GetStaticId();
    }

    public static byte GetStaticId() {
        return (byte)EventType.Shot;
    }
}
