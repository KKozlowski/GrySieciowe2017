using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateEvent : EventBase {

    public PlayerState state = new PlayerState();

    public override void Deserialize(ByteStreamReader reader) {
        state.Deserialize(reader);
    }

    public override void Serialize(ByteStreamWriter writer) {
        state.Serialize(writer);
    }

    public override byte GetId() {
        //return InputEvent.GetStaticId();
        return PlayerStateEvent.GetStaticId();
    }

    public static byte GetStaticId() {
        return (byte)EventType.PlayerState;
    }
}
