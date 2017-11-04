using System;
using System.Collections.Generic;
using UnityEngine;

class PlayerPawn
{
    Vector2 m_position;
    float m_radius;
    float m_power;

    public void Update( float dt )
    {
    }

    public float ReceiveDamage( float dmg )
    {
        float powerIncome = 0.0f;

        if ( m_power < dmg )
        {
            // death
            powerIncome = m_power;
        }
        else
        {
            m_power -= dmg;
        }

        powerIncome *= Mathf.Exp( -dmg / 15 );

        return powerIncome;
    }

    public float GetPower() { return m_power; }

    public bool Intersects( SimpleRay2D ray )
    {
        Vector2 L = m_position - ray.origin;
        float tca = Vector2.Dot( L, ray.direction );

        if( tca < 0 )
            return false;

        float d2 = Vector2.Dot( L, L ) - tca * tca;
        if ( d2 > m_radius * m_radius )
            return false;

        float thc = m_radius - Mathf.Sqrt( d2 );
        float t0 = tca - thc;
        float t1 = tca + thc;

        if ( t0 > t1 )
        {
            if ( t0 > ray.length )
                return false;
        }
        else
        {
            if ( t1 > ray.length )
                return false;
        }

        return true;
    }

    public void AddPower( float power )
    {
        m_power += power;
    }
}

class SimpleRay2D
{
    public Vector2 origin;
    public Vector2 direction;
    public float length;
}

class World
{
    List< PlayerPawn > m_players = new List<PlayerPawn>();

    public void Init()
    {
    }

    public void Update( float dt )
    {
        for ( int i = 0; i < m_players.Count; ++i )
        {
            m_players[i].Update( dt );
        }
    }

    public PlayerPawn CreatePlayer( PlayerSession session )
    {
        PlayerPawn pawn = new PlayerPawn();

        return pawn;
    }

    void ShootLaser( SimpleRay2D ray, PlayerPawn owner )
    {
        List< PlayerPawn > playersHit = CastRay( ray );
        float laserPower = owner.GetPower();
        float powerAccumulator = 0.0f;
        for ( int i = 0; i < playersHit.Count; ++i )
        {
            if ( playersHit[i] == owner )
                continue;

            powerAccumulator += playersHit[i].ReceiveDamage( laserPower );
        }

        owner.AddPower( powerAccumulator );
    }

    List< PlayerPawn > CastRay( SimpleRay2D ray )
    {
        List< PlayerPawn > result = new List<PlayerPawn>();
        for ( int i = 0; i < m_players.Count; ++i )
        {
            if ( m_players[ i ].Intersects( ray ) )
            {
                result.Add( m_players[i] );
            }
        }

        return result;
    }
}