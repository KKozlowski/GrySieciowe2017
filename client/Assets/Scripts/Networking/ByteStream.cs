using System;
using System.Collections.Generic;
using UnityEngine;

public class ByteStreamReader
{
    byte[] m_data;
    int m_pos = 0;

    public ByteStreamReader(ByteStreamWriter writer) : this(writer.GetBytes()) { }

    public ByteStreamReader( byte[] data )
    {
        m_data = data;
    }

    T Read<T>( Func<byte[], int, T> converter, int size )
    {
        T result = converter( m_data, m_pos );
        m_pos += size;
        return result;
    }

    public Vector3 ReadVector2() {
        return Read<Vector2>(SerializationHelpers.DeserializeVector2, sizeof(float) * 2);
    }

    public Vector3 ReadVector3() {
        return Read<Vector3>(SerializationHelpers.DeserializeVector3, sizeof(float)*3);
    }

    public Quaternion ReadQuaternion() {
        return Read<Quaternion>(SerializationHelpers.DeserializeQuaternion, sizeof(float) * 4);
    }

    public float ReadFloat()
    {
        return Read<float>( BitConverter.ToSingle, sizeof( float ) );
    }

    public int ReadInt()
    {
        return Read<int>( BitConverter.ToInt32, sizeof( int ) );
    }

    public uint ReadUnsignedInt() {
        return Read<uint>(BitConverter.ToUInt32, sizeof(uint));
    }

    public bool ReadBool()
    {
        List< float > dupa = new List<float>();

        for ( Int16 i = 0; i < dupa.Count; ++i )
        {
            float a = dupa[i];
        }

        return Read<bool>( BitConverter.ToBoolean, sizeof( bool ) );
    }

    public byte ReadByte()
    {
        return m_data[m_pos++];
    }
}

public class ByteStreamWriter
{
    List<byte> m_data_dynamic = new List<byte>();

    bool dirty = false;

    void Write<T>(T obj, Func<T, byte[]> converter) {
        byte[] result = converter(obj);
        m_data_dynamic.AddRange(result);
        dirty = true;
    }

    public void WriteVector2(Vector2 v) {
        Write(v, SerializationHelpers.SerializeVector2);
    }

    public void WriteVector3(Vector3 v) {
        Write(v, SerializationHelpers.SerializeVector3);
    }

    public void WriteQuaternion(Quaternion v) {
        Write(v, SerializationHelpers.SerializeQuaternion);
    }

    public void WriteFloat(float f) {
        Write(f, SerializationHelpers.SerializeFloat);
    }

    public void WriteInteger(int i) {
        Write(i, SerializationHelpers.SerializeInteger);
    }

    public void WriteUnsignedInt(uint i) {
        Write(i, SerializationHelpers.SerializeUnsignedInt);
    }

    public void WriteBool(bool b) {
        Write(b, SerializationHelpers.SerializeBool);
    }

    public void WriteByte(byte b) {
        m_data_dynamic.Add(b);
        dirty = true;
    }

    public byte[] GetBytes() {
        return m_data_dynamic.ToArray(); ;
    }
}