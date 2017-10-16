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
    StreamingEvent = 1 << 3
}

public class NetEventsFactory
{
    MemoryStream m_stream;

    public const int c_minPacketSize = 5; // flags + size

    public bool SetData( byte[] data )
    {
        System.Diagnostics.Debug.Assert( data.Length >= c_minPacketSize );
        m_stream = new MemoryStream( data );


        return true;
    }

    bool HasFlag( byte flag, byte value )
    {
        return ( flag & value ) != 0;
    }
}