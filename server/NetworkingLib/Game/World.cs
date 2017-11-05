using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPawn
{
    Vector2 m_position;
    float m_radius;
    float m_power;
    float m_movementSpeed = 1;
    float m_laserLength = 5;

    Vector2 m_movementDirection = Vector2.zero;

    public bool isPlayingNow = true;
    public DateTime lastDeathTime;

    World world;
    
    public bool IsAlive { get; private set; }
    public event Action<PlayerPawn> OnDeath;

    public void Update( float dt )
    {
        m_position += m_movementDirection * dt * m_movementSpeed;
    }

    public void Move(Vector2 direction) {
        m_movementDirection = direction;
    }

    public void ShootAtDirection(Vector2 direction) {
        SimpleRay2D ray = new SimpleRay2D();
        ray.origin = m_position;
        ray.direction = direction;
        ray.length = m_laserLength;
        world.ShootLaser(ray, this);
    }

    public float ReceiveDamage( float dmg )
    {
        float powerIncome = 0.0f;

        if ( m_power < dmg )
        {
            // death
            powerIncome = m_power;
            Die();
        }
        else
        {
            AddPower(-dmg);
            powerIncome = dmg;
        }

        powerIncome *= Mathf.Exp( -dmg / 15 );

        return powerIncome;
    }

    public void Die() {
        m_power = 0;
        IsAlive = false;
        lastDeathTime = DateTime.Now;

        if (OnDeath != null)
            OnDeath(this);
    }

    public void Respawn(Vector2 position) {
        IsAlive = true;
        m_position = position;
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

    public PlayerPawn(World w, Vector2 position) {
        world = w;
        m_position = position;
        lastDeathTime = new DateTime();
    }
}

public class SimpleRay2D
{
    public Vector2 origin;
    public Vector2 direction;
    public float length;
}

public class World
{
    Dictionary< PlayerSession, PlayerPawn > m_players 
        = new Dictionary<PlayerSession, PlayerPawn>();

    public int respawnTime = 5;

    public void Init()
    {

    }

    public void Update(float dt) {
        foreach (var kvp in m_players) {
            if (kvp.Value.IsAlive)
                kvp.Value.Update(dt);
        }

        RespawnLoop();
    }

    public PlayerPawn AddPlayer( PlayerSession session )
    {
        PlayerPawn pawn = CreatePawn();
        m_players[session] = pawn;

        return pawn;
    }
    public void RemovePlayer( PlayerSession session ) {
        m_players.Remove(session);
    }

    private void OnPawnDeath( PlayerPawn pawn ) {
        PlayerSession session = m_players.Where(x => x.Value == pawn).FirstOrDefault().Key;
        if (session != null) {
            Console.WriteLine("Some pawn Died");
            //Send death event
        }
    }

    public bool IsPlayerAlive( PlayerSession session) {
        PlayerPawn pawn = null;
        m_players.TryGetValue(session, out pawn);
        return pawn != null && pawn.IsAlive;
    }

    public bool IsPlayerInWorld(PlayerSession session) {
        return m_players.ContainsKey(session);
    }

    public void KillPlayer( PlayerSession session ) {
        PlayerPawn pawn = null;
        m_players.TryGetValue(session, out pawn);
        if (pawn != null) {
            pawn.Die();
        }
    }

    public void RespawnPlayer( PlayerSession session ) {
        m_players[session].Respawn(Vector2.zero);
    }

    public void RespawnLoop() {
        foreach (var kvp in m_players.Where(x=>x.Value == null)) {
            if (kvp.Value.isPlayingNow 
                && (DateTime.Now - kvp.Value.lastDeathTime).Seconds >= respawnTime) {
                RespawnPlayer(kvp.Key);
            }
        }
    }

    private PlayerPawn CreatePawn() {
        PlayerPawn pawn = new PlayerPawn(this, Vector2.zero);
        pawn.OnDeath += OnPawnDeath;
        return pawn;
    }

    public void ShootLaser( SimpleRay2D ray, PlayerPawn owner )
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
        foreach(var kvp in m_players) {
            if (kvp.Value != null) {
                if (kvp.Value.Intersects(ray)) {
                    result.Add(kvp.Value);
                }
            }
        }

        return result;
    }
}