using System;
using System.Collections.Generic;

public class ByteStreamReader
{
    byte[] m_data;
    int m_pos = 0;

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

    public float ReadFloat()
    {
        return Read<float>( BitConverter.ToSingle, sizeof( float ) );
    }

    public int ReadInt()
    {
        return Read<int>( BitConverter.ToInt32, sizeof( int ) );
    }

    public bool ReadBool()
    {
        return Read<bool>( BitConverter.ToBoolean, sizeof( bool ) );
    }

    public byte ReadByte()
    {
        return m_data[m_pos++];
    }
}

public class ByteStreamWriter
{
}