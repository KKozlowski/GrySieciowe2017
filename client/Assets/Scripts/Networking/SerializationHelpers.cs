using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class SerializationHelpers {

    public static byte[] SerializeInteger(int i) {
        return i.Serialize();
    }

    public static byte[] SerializeUnsignedInt(uint i) {
        return i.Serialize();
    }

    public static byte[] SerializeShort(short i) {
        return i.Serialize();
    }

    public static byte[] SerializeUnsignedShort(ushort i) {
        return i.Serialize();
    }

    public static byte[] SerializeLong(long i) {
        return i.Serialize();
    }

    public static byte[] SerializeUnsignedLong(ulong i) {
        return i.Serialize();
    }

    public static byte[] SerializeBool(bool b) {
        return b.Serialize();
    }

    public static byte[] SerializeFloat(float i) {
        return i.Serialize();
    }

    public static byte[] SerializeVector2(Vector2 i) {
        return i.Serialize();
    }

    public static byte[] SerializeVector3(Vector3 i) {
        return i.Serialize();
    }

    public static byte[] SerializeQuaternion(Quaternion i) {
        return i.Serialize();
    }

    public static byte[] Serialize(this int i) {
        return BitConverter.GetBytes(i);
    }

    public static int DeserializeInteger(this byte[] bytes, int offset = 0) {
        return BitConverter.ToInt32(bytes, offset);
    }

    public static byte[] Serialize(this uint i) {
        return BitConverter.GetBytes(i);
    }
    public static uint DeserializeUnsignedInt(this byte[] bytes, int offset = 0) {
        return BitConverter.ToUInt32(bytes, offset);
    }

    public static byte[] Serialize(this short i) {
        return BitConverter.GetBytes(i);
    }
    public static short DeserializeShort(this byte[] bytes, int offset = 0) {
        return BitConverter.ToInt16(bytes, offset);
    }

    public static byte[] Serialize(this ushort i) {
        return BitConverter.GetBytes(i);
    }
    public static ushort DeserializeUnsignedShort(this byte[] bytes, int offset = 0) {
        return BitConverter.ToUInt16(bytes, offset);
    }

    public static byte[] Serialize(this long i) {
        return BitConverter.GetBytes(i);
    }
    public static long DeserializeLong(this byte[] bytes, int offset = 0) {
        return BitConverter.ToInt64(bytes, offset);
    }

    public static byte[] Serialize(this ulong i) {
        return BitConverter.GetBytes(i);
    }
    public static ulong DeserializeUnsignedLong(this byte[] bytes, int offset = 0) {
        return BitConverter.ToUInt64(bytes, offset);
    }

    public static byte[] Serialize(this bool i) {
        return BitConverter.GetBytes(i);
    }
    public static bool DeserializeBool(this byte[] bytes, int offset = 0) {
        return BitConverter.ToBoolean(bytes, offset);
    }

    public static byte[] Serialize(this float i) {
        return BitConverter.GetBytes(i);
    }
    public static float DeserializeFloat(this byte[] bytes, int offset = 0) {
        return BitConverter.ToSingle(bytes, offset);
    }

    public static byte[] Serialize(this Vector2 vector) {
        byte[] bytes = new byte[8];
        Array.Copy(BitConverter.GetBytes(vector.x), 0, bytes, 0, 4);
        Array.Copy(BitConverter.GetBytes(vector.y), 0, bytes, 4, 4);
        return bytes;
    }

    public static Vector2 DeserializeVector2(this byte[] bytes, int offset = 0) {
        return new Vector2(
            BitConverter.ToSingle(bytes, offset),
            BitConverter.ToSingle(bytes, offset+4));
    }

    public static byte [] Serialize(this Vector3 vector) {
        byte[] bytes = new byte[12];
        Array.Copy(BitConverter.GetBytes(vector.x), 0, bytes, 0, 4);
        Array.Copy(BitConverter.GetBytes(vector.y), 0, bytes, 4, 4);
        Array.Copy(BitConverter.GetBytes(vector.z), 0, bytes, 8, 4);
        return bytes;
    }

    public static Vector3 DeserializeVector3(this byte[] bytes, int offset = 0) {
        return new Vector3(
            BitConverter.ToSingle(bytes, offset),
            BitConverter.ToSingle(bytes, offset+4),
            BitConverter.ToSingle(bytes, offset+8));
    }

    public static byte[] Serialize(this Quaternion vector) {
        byte[] bytes = new byte[16];
        Array.Copy(BitConverter.GetBytes(vector.x), 0, bytes, 0, 4);
        Array.Copy(BitConverter.GetBytes(vector.y), 0, bytes, 4, 4);
        Array.Copy(BitConverter.GetBytes(vector.z), 0, bytes, 8, 4);
        Array.Copy(BitConverter.GetBytes(vector.w), 0, bytes, 12, 4);
        return bytes;
    }

    public static Quaternion DeserializeQuaternion(this byte[] bytes, int offset = 0) {
        return new Quaternion(
            BitConverter.ToSingle(bytes, offset),
            BitConverter.ToSingle(bytes, offset+4),
            BitConverter.ToSingle(bytes, offset+8),
            BitConverter.ToSingle(bytes, offset+12));
    }
}