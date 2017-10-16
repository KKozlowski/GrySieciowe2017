using System;
using System.Collections;
using System.Collections.Generic;

public enum EventType : byte {
    Shot = 10,
    Death = 255
}

public interface IEvent
{
    uint SelfId { get; }
    EventType TypeId { get; }
    string ToString();
}

public abstract class EventBase : IEvent {
    private uint id;
    public uint SelfId { get { return id; } }

    public EventBase(uint id) {
        this.id = id;
    }

    public abstract new string ToString();
    public abstract EventType TypeId { get; }
}

public class ShotEvent : EventBase {
    public ShotEvent(uint id) : base(id) {
    }

    public override EventType TypeId
    {
        get
        {
            return EventType.Shot;
        }
    }

    public override string ToString() {
        return "ShotEvent";
    }
}