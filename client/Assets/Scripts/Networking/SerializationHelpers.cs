using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class SerializationHelpers {

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