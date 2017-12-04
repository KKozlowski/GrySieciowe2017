using System;
using System.Collections.Generic;

/// <summary>
/// Simple class that gives incremented unique ids.
/// </summary>
public class IdAllocator
{
    private int m_next = 0;

    /// <summary>
    /// Gets new unique id.
    /// </summary>
    /// <returns>Unique integer id.</returns>
    public int Allocate()
    {
        return m_next++;
    }
}