using System;
using System.Collections.Generic;

public class IdAllocator
{
    private int m_next = 0;

    public int Allocate()
    {
        return m_next++;
    }
}