using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateEvent : EventBase {

    public List<PlayerState> states = new List<PlayerState>();

    public override void Deserialize(ByteStreamReader reader)
    {
        int count = reader.ReadInt();
        for (int i = 0; i < count; i++)
        {
            PlayerState ps = new PlayerState();
            ps.Deserialize(reader);
            states.Add(ps);
        }
        //Network.Log("ID: " + state.id + ", position: " + state.position);
    }

    public override void Serialize(ByteStreamWriter writer) {
        writer.WriteInteger(states.Count);
        foreach (PlayerState playerState in states)
        {
            playerState.Serialize(writer);
        }
    }

    public override byte GetId() {
        //return InputEvent.GetStaticId();
        return PlayerStateEvent.GetStaticId();
    }

    public static byte GetStaticId() {
        return (byte)EventType.PlayerState;
    }
}
