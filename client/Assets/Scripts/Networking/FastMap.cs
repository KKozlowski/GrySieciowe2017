using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class FastMap<T>
    where T : class
{
    List<int>   m_keys = new List<int>();
    List<T>     m_values = new List<T>();

    public void Add( int id, T val )
    {
        m_keys.Add( id );
        m_values.Add( val );
    }

    public bool AddUnique( int id, T val )
    {
        if ( Find( id ) != null )
            return false;

        Add( id, val );
        return true;
    }

    public int FindIndex( int key )
    {
        int count = m_keys.Count;
        for ( int i = 0; i < count; ++i )
        {
            if ( m_keys[i] == key )
            {
                return i;
            }
        }

        return -1;
    }

    public override int GetHashCode()
    {
        StringBuilder sb = new StringBuilder( m_keys.Count * 2 );
        for ( int i = 0; i < m_keys.Count; ++i )
        {
            sb.Append( m_keys[i] );
            sb.Append( m_values[i] );
        }
        string str = sb.ToString();
        return str.GetHashCode();
    }

    public T Find( int key )
    {
        int count = m_keys.Count;
        for ( int i = 0; i < count; ++i )
        {
            if ( m_keys[i] == key )
            {
                return m_values[i];
            }
        }

        return null;
    }

    public bool Remove( T val )
    {
        int c = m_values.Count;
        for ( int i = 0; i < c; ++i )
        {
            if ( val == m_values[i] )
            {
                m_values.RemoveAt( i );
                m_keys.RemoveAt( i );
                return true;
            }
        }

        return false;
    }

    public void RemoveAt( int index )
    {
        m_keys.RemoveAt( index );
        m_values.RemoveAt( index );
    }

    public int GetKeyAt( int index )
    {
        return m_keys[index];
    }

    public T GetValueAt( int index )
    {
        return m_values[index];
    }

    public bool RemoveAtKey( int key )
    {
        int c = m_keys.Count;
        for ( int i = 0; i < c; ++i )
        {
            if ( key == m_keys[i] )
            {
                m_keys.RemoveAt( i );
                m_values.RemoveAt( i );
                return true;
            }
        }

        return false;
    }

    public int Count() { return m_keys.Count; }

    internal void Clear()
    {
        m_keys.Clear();
        m_values.Clear();
    }
}

public class FastMapId<T>
{
    IdAllocator m_id = new IdAllocator();
    List<int>   m_keys = new List<int>();
    List<T>     m_values = new List<T>();

    public int Add( T val )
    {
        int id = m_id.Allocate();

#if UNITY_EDITOR
        Debug.Assert( m_keys.Count == m_values.Count );
#endif
        m_keys.Add( id );
        m_values.Add( val );

        return id;
    }

    public bool Remove( int key )
    {
        int sz = m_keys.Count;
        for ( int i = 0; i < sz; ++i )
        {
            if ( m_keys[i] == key )
            {
                m_keys.RemoveAt( i );
                m_values.RemoveAt( i );
                return true;
            }
        }

        return false; // not found
    }
}
