using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class PlayerState {
    public int id;

    public float power;
    public Vector2 position;

    public const int maskOfHealthChange = 1 << 1;
    public const int maskOfPositionChange = 1;

    bool healthDirty = false;
    bool positionDirty = false;

    public void SetHealthDirty(bool dirty) {
        healthDirty = dirty;
    }

    public void SetPositionDirty(bool dirty) {
        positionDirty = dirty;
    }

    public void Serialize(ByteStreamWriter writer) {
        writer.WriteInteger(id);

        int changeMask = 0;
        if (healthDirty)
            changeMask = changeMask | maskOfHealthChange;
        if (positionDirty)
            changeMask = changeMask | maskOfPositionChange;

        writer.WriteByte((byte)changeMask);

        if (healthDirty) {
            writer.WriteFloat(power);
        }
        if (positionDirty) {
            writer.WriteVector2(position);
        }
    }

    public bool Deserialize(ByteStreamReader reader) {
        int id = reader.ReadInt();

        int changeMask = reader.ReadByte();

        bool changedPosition = (changeMask & maskOfPositionChange) != 0,
            changedHealth = (changeMask & maskOfHealthChange) != 0;

        if (changedHealth) {
            power = reader.ReadFloat();
            Console.WriteLine("Applying health: " + power);
        }
        if (changedPosition) {
            position = reader.ReadVector2();
            Console.WriteLine("Applying position: " + position);
        }

        return true;
    }
}