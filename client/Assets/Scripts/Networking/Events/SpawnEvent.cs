﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRequestEvent : EventBase
{
    public int m_sessionId;
    public bool m_actuallySpawned;
    public Vector2 m_startPosition;

    public SpawnRequestEvent() { }

    public SpawnRequestEvent(int sessionId, bool actuallySpawned = false) {
        m_sessionId = sessionId;
        m_actuallySpawned = actuallySpawned;
    }

    public override void Serialize( ByteStreamWriter writer )
    {
        writer.WriteInteger( m_sessionId );
        writer.WriteBool(m_actuallySpawned);
        if (m_actuallySpawned)
            writer.WriteVector2(m_startPosition);
    }

    public override void Deserialize( ByteStreamReader reader )
    {
        m_sessionId = reader.ReadInt();
        m_actuallySpawned = reader.ReadBool();
        if (m_actuallySpawned)
            m_startPosition = reader.ReadVector2();
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