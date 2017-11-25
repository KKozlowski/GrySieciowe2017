using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class ReliableEventResponse : EventBase {
    public int m_reliableEventId = -1;

    public override void Serialize(ByteStreamWriter writer) {
        writer.WriteInteger(m_reliableEventId);
    }

    public override void Deserialize(ByteStreamReader reader)
    {
        m_reliableEventId = reader.ReadInt();
    }

    public override byte GetId() {
        return GetStaticId();
    }

    public static byte GetStaticId() {
        return (byte)EventType.ReliableEventResponse;
    }
}

