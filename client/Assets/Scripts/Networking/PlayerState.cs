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

    bool healthDirty = false;
    bool positionDirty = false;

    public void SetHealthDirty( bool dirty )
    {
        healthDirty = dirty;
    }

    public void SetPositionDirty( bool dirty )
    {
        positionDirty = dirty;
    }

    public void Serialize( ByteStreamWriter writer )
    {
        writer.WriteUnsignedInt( id );

        int changeMask = 0;
        if ( healthDirty )
            changeMask = changeMask | maskOfHealthChange;
        if ( positionDirty )
            changeMask = changeMask | maskOfPositionChange;

        writer.WriteByte( ( byte )changeMask );

        if ( healthDirty )
        {
            writer.WriteInteger( health );
        }
        if ( positionDirty )
        {
            writer.WriteVector2( position );
        }
    }

    public bool Deserialize(ByteStreamReader reader) {
        uint thatID = reader.ReadUnsignedInt();
        if (id != thatID)
            return false;

        int changeMask = reader.ReadByte();

        bool changedPosition = (changeMask & maskOfPositionChange) != 0,
            changedHealth = (changeMask & maskOfHealthChange) != 0;

        if (changedHealth) {
            health = reader.ReadInt();
            Console.WriteLine("Applying health: " + health);
        }
        if (changedPosition) {
            position = reader.ReadVector2();
            Console.WriteLine("Applying position: " + position);
        }

        return true;
    }
}
