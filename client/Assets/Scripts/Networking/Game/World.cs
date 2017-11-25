using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class PlayerPawn
{
    Vector2 m_position;
    float m_radius;
    float m_power = 1;
    float m_movementSpeed = 1;
    float m_laserLength = 5;

    Vector2 m_movementDirection = Vector2.zero;

    public bool m_isPlayingNow = false;
    public DateTime m_lastDeathTime;

    World world;
    
    public bool IsAlive { get; private set; }
    public event Action<PlayerPawn> OnDeath;

    public float Power { get { return m_power; } }
    public Vector2 Position { get { return m_position; } }

    public int Id { get; private set; }

    public void Update( float dt )
    {
        m_position += m_movementDirection * dt * m_movementSpeed;
    }

    public void Move(Vector2 direction)
    {
        Vector2 normalized = direction.normalized;
        if (m_movementDirection != normalized)
        {
            m_movementDirection = normalized;
            Console.WriteLine("New direction for "+ Id+ ": " + m_movementDirection.ToString());
        }
        
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
        m_lastDeathTime = DateTime.Now;

        if (OnDeath != null)
            OnDeath(this);
    }

    public void Respawn(Vector2 position) {
        IsAlive = true;
        m_position = position;
        m_power = 1;
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

    public PlayerPawn(World w, Vector2 position, int id) {
        world = w;
        m_position = position;
        m_lastDeathTime = new DateTime();
        Id = id;
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
    public class ProperInputListener : IEventListener {
        World m_world = null;
        public ProperInputListener(World w) {
            m_world = w;
        }

        public bool Execute(EventBase e) {
            InputEvent input = (InputEvent)e;

            PlayerPawn pawn = m_world.TryGetPawn(input.m_sessionId);
            //Console.WriteLine("Looking for pawn with id " + input.m_sessionId + ": " + pawn);
            if (pawn != null) {
                pawn.Move(input.m_direction);
            } else {
                Console.WriteLine("InputEvent: Target pawn doesn't exist");
            }

            return true;
        }

        public EventType GetEventType() {
            return (EventType)InputEvent.GetStaticId();
        }
    }

    public class ProperPleaseSpawnListener : IEventListener {
        World m_world = null;
        public ProperPleaseSpawnListener(World w) {
            m_world = w;
        }

        public bool Execute(EventBase e) {
            SpawnRequestEvent input = (SpawnRequestEvent)e;

            PlayerPawn pawn = m_world.TryGetPawn(input.m_sessionId);
            //Console.WriteLine("Looking for pawn with id " + input.m_sessionId + ": " + pawn);
            if (pawn!= null && !pawn.m_isPlayingNow) {
                pawn.m_isPlayingNow = true;
                m_world.RespawnPlayer(input.m_sessionId);
            }

            return true;
        }

        public EventType GetEventType() {
            return (EventType)SpawnRequestEvent.GetStaticId();
        }
    }

    Dictionary< int, PlayerPawn > m_players 
        = new Dictionary<int, PlayerPawn>();

    public int respawnTime = 5;

    ProperInputListener m_inputListener = null;// new ProperInputListener();
    ProperPleaseSpawnListener m_spawnListener = null;

    System.Random m_radom = new System.Random();

    Thread updateThread;

    public void Init()
    {
        m_inputListener = new ProperInputListener(this);
        m_spawnListener = new ProperPleaseSpawnListener(this);
        Network.AddListener(m_inputListener);
        Network.AddListener(m_spawnListener);

        updateThread = new Thread(UpdateLoop);
        updateThread.Start();
    }

    ~World() {
        if (updateThread != null) {
            updateThread.Interrupt();
            Network.Log("Update loop broken");
        }
            
    }

    void UpdateLoop() {
        DateTime lastTime = DateTime.Now;
        while (true) {
            Thread.Sleep(16);
            DateTime thisTime = DateTime.Now;
            float dt = (thisTime - lastTime).Milliseconds * 0.001f;
            lastTime = thisTime;
            foreach (var kvp in m_players) {
                kvp.Value.Update(dt);
            }
            //Network.Log("UPDATE TICK");
            try {
                foreach (int id in m_players.Keys) {
                    SendStatesToPlayer(id);
                    //Network.Log("SENDING STATE");
                }
            } catch (InvalidOperationException) {
                Network.Log("Concurrency problem.");
            }
            
        }
    }

    private PlayerPawn TryGetPawn(int id) {
        PlayerPawn pawn = null;
        m_players.TryGetValue(id, out pawn);
        return pawn;
    }

    public void Update(float dt) {
        foreach (var kvp in m_players) {
            if (kvp.Value.IsAlive)
                kvp.Value.Update(dt);
        }

        RespawnLoop();
    }

    public PlayerPawn AddPlayer(int sessionId )
    {
        PlayerPawn pawn = CreatePawn(sessionId);
        m_players[sessionId] = pawn;
        Console.WriteLine("Player pawn added with id: " + sessionId);
        return pawn;
    }
    public void RemovePlayer(int sessionId ) {
        m_players.Remove(sessionId);
    }

    private void OnPawnDeath( PlayerPawn pawn ) {
        var found = m_players.Where(x => x.Value == pawn);
        if (found.Count() != 0) {
            int id = found.ElementAt(0).Key;
            Console.WriteLine("Some pawn Died");
            //Send death event
        }
    }

    public void SendStatesToPlayer(int playerConnectionId)
    {
        PlayerStateEvent e = new PlayerStateEvent();
        foreach (var kvp in m_players)
        {
            PlayerPawn pawn = kvp.Value;
            PlayerState ps = new PlayerState();
            ps.id = pawn.Id;
            ps.power = pawn.Power;
            ps.SetHealthDirty(true);
            ps.position = pawn.Position;
            ps.SetPositionDirty(true);

            e.states.Add(ps);
        }

        Network.Server.Send(e, playerConnectionId);
    }

    //public void SendPawnStateToPlayer(int playerConnectionId, PlayerPawn pawn) {
    //    PlayerState ps = new PlayerState();
    //    ps.id = pawn.Id;
    //    ps.power = pawn.Power;
    //    ps.SetHealthDirty(true);
    //    ps.position = pawn.Position;
    //    ps.SetPositionDirty(true);

    //    PlayerStateEvent e = new PlayerStateEvent();
    //    e.state = ps;
    //    Network.Log("Sending state of " + ps.id + " (pos: " + e.state.position.ToString() + ") to " + playerConnectionId);
    //    Network.Server.Send(e, playerConnectionId);
        
    //}

    public bool IsPlayerAlive(int sessionId) {
        PlayerPawn pawn = null;
        m_players.TryGetValue(sessionId, out pawn);
        return pawn != null && pawn.IsAlive;
    }

    public bool IsPlayerInWorld(int sessionId) {
        return m_players.ContainsKey(sessionId);
    }

    public void KillPlayer(int sessionId ) {
        PlayerPawn pawn = null;
        m_players.TryGetValue(sessionId, out pawn);
        if (pawn != null) {
            pawn.Die();
        }
    }

    private Vector2 GetRandomPoint(double rMax) {

            var r = Math.Sqrt((double)m_radom.Next() / int.MaxValue) * rMax;
            var theta = (double)m_radom.Next() / int.MaxValue * 2 * Math.PI;
            return new Vector2((float)(r * Math.Cos(theta)), (float)(r * Math.Sin(theta)));
    }

    public void RespawnPlayer( int sessionId ) {
        PlayerPawn pawn = null;
        m_players.TryGetValue(sessionId, out pawn);
        if (pawn != null) {
            Vector2 position = GetRandomPoint(5);
            pawn.Respawn(position);

            var spawnEvent = new SpawnRequestEvent(sessionId, true);
            spawnEvent.m_startPosition = position;
            Network.Log("Pawn " + sessionId + " respawned at " + position);
            Network.Server.Send(spawnEvent, sessionId);
        }
    }

    public void RespawnLoop() {
        foreach (var kvp in m_players.Where(x=>x.Value == null)) {
            if (kvp.Value.m_isPlayingNow 
                && (DateTime.Now - kvp.Value.m_lastDeathTime).Seconds >= respawnTime) {
                RespawnPlayer(kvp.Key);
            }
        }
    }

    private PlayerPawn CreatePawn(int connectionId) {
        PlayerPawn pawn = new PlayerPawn(this, Vector2.zero, connectionId);
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