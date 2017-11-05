using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRequestEvent : EventBase
{
    int m_playerId;

    public override void Serialize( ByteStreamWriter writer )
    {
        writer.WriteInteger( m_playerId );
    }

    public override void Deserialize( ByteStreamReader reader )
    {
        m_playerId = reader.ReadInt();
    }

    public override byte GetId()
    {
        return GetStaticId();
    }

    public static byte GetStaticId()
    {
        return (byte)EventType.SpawnRequest;
    }
}