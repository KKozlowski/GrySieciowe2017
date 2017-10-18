using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum MsgFlags : byte
{
    ConnectionRequest = 1 << 0,
    UnreliableEvent = 1 << 1,
    ReliableEvent = 1 << 2,
    State = 1 << 3
}

public class NetEventsFactory
{
    ByteStreamReader m_stream;

    public const int c_minPacketSize = 5; // flags + size

    public NetEventsFactory()
    {

    }

    public bool DeserializeData( byte[] data )
    {
        System.Diagnostics.Debug.Assert( data.Length >= c_minPacketSize );
        m_stream = new ByteStreamReader( data );

        byte flags = m_stream.ReadByte();

        if ( HandleEvent( m_stream, flags ) )
        {
            return true;
        }

        return false;
    }

    bool HandleEvent( ByteStreamReader stream, byte flags )
    {
        if ( HasFlag( flags, ( byte )MsgFlags.UnreliableEvent ) )
        {
            return true;
        }
        else if ( HasFlag( flags, ( byte )MsgFlags.ReliableEvent ) )
        {
            return true;
        }

        return false;
    }

    bool HasFlag( byte flag, byte value )
    {
        return ( flag & value ) != 0;
    }
}