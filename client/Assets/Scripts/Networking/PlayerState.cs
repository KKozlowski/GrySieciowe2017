using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class PlayerState  {
    public int id;

    public int health;
    public Vector2 position;

    public const int maskOfHealthChange = 1 << 1;
    public const int maskOfPositionChange = 1;

    public ByteStreamWriter ConstructMessage(bool changedPosition, bool changedHealth) {
        byte[] header = new byte[5];
        Array.Copy(id.Serialize(), header, 4);
        ByteStreamWriter stream = new ByteStreamWriter();
        stream.WriteInteger(id);

        int changeMask = 0;
        if (changedHealth)
            changeMask = changeMask | maskOfHealthChange;
        if (changedPosition)
            changeMask = changeMask | maskOfPositionChange;

        stream.WriteByte((byte)changeMask);

        if (changedHealth) {
            stream.WriteInteger(health);
        }
        if (changedPosition) {
            stream.WriteVector2(position);
        }

        return stream;
    }

    public bool ApplyMessage(ByteStreamReader bytes) {
        int thatID = bytes.ReadInt();
        if (id != thatID)
            return false;

        int changeMask = bytes.ReadByte();

        bool changedPosition = (changeMask & maskOfPositionChange) != 0,
            changedHealth = (changeMask & maskOfHealthChange) != 0;

        if (changedHealth) {
            health = bytes.ReadInt();
            Console.WriteLine("Applying health: " + health);
        }
        if (changedPosition) {
            position = bytes.ReadVector2();
            Console.WriteLine("Applying position: " + position);
        }

        return true;
    }
}
