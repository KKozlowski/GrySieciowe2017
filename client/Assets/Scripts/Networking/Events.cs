using System;
using System.Collections;
using System.Collections.Generic;

public enum EventType : byte
{
}

public abstract class EventBase
{
    public abstract EventType GetEventType();
}