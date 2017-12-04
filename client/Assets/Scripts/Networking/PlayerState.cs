using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

/// <summary>
/// All the data that need to be sent from server to client about given player. Contains dirty/notDirty mechanism
/// for sending only the changed data.
/// </summary>
public class PlayerState {
    /// <summary>
    /// Player's connection identificator.
    /// </summary>
    public int id;

    /// <summary>
    /// The player's power (a.k.a. health and fire power)
    /// </summary>
    public float power;

    /// <summary>
    /// The position of the player.
    /// </summary>
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
        id = reader.ReadInt();

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

    /// <summary>
    /// Gets the radius the player should have based on his power
    /// </summary>
    /// <param name="power">The power.</param>
    /// <returns>The calculated radius</returns>
    public static float GetRadiusByPower(float power)
    {
        return 0.75f + Mathf.Sqrt(power)*0.5f;
    }
}