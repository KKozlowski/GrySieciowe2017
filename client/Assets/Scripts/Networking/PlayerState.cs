using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class PlayerState  {
    public uint id;

    public int health;
    public Vector2 position;

    public const int maskOfHealthChange = 1 << 1;
    public const int maskOfPositionChange = 1;

    public byte [] ConstructMessage(bool changedPosition, bool changedHealth) {
        byte[] header = new byte[5];
        Array.Copy(id.Serialize(), header, 4);

        int changeMask = 0;
        if (changedHealth)
            changeMask = changeMask | maskOfHealthChange;
        if (changedPosition)
            changeMask = changeMask | maskOfPositionChange;
        header[4] = (byte)changeMask;

        byte[] result = header;

        if (changedHealth) {
            result = result.Concat(health.Serialize()).ToArray();
        }
        if (changedPosition) {
            result = result.Concat(position.Serialize()).ToArray();
        }

        return result;
    }

    public bool ApplyMessage(byte[] bytes) {
        uint thatID = bytes.DeserializeUnsignedInt();
        if (id != thatID)
            return false;

        int changeMask = bytes[4];

        bool changedPosition = (changeMask & maskOfPositionChange) != 0,
            changedHealth = (changeMask & maskOfHealthChange) != 0;

        int index = 5;
        if (changedHealth) {
            health = bytes.DeserializeInteger(index);
            Console.WriteLine("Applying health: " + health);
            index += 4;
        }
        if (changedPosition) {
            position = bytes.DeserializeVector2(index);
            Console.WriteLine("Applying position: " + position);
            index += 8;
        }

        return true;
    }
}
