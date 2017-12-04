using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

/// <summary>
/// Player's data and behaviors in the World.
/// </summary>
public class PlayerPawn
{
    Vector2 m_position;
    float Radius {get { return PlayerState.GetRadiusByPower(m_power); } }
    float m_power = 0;
    float m_movementSpeed = 1;
    float m_laserLength = 12.5f;

    Vector2 m_movementDirection = Vector2.zero;

    public bool m_isPlayingNow = false;
    public DateTime m_lastDeathTime;

    World world;
    
    public bool IsAlive { get; private set; }
    public event Action<PlayerPawn> OnDeath;

    public float Power { get { return m_power; } }
    public Vector2 Position { get { return m_position; } }

    public DateTime m_lastPackageTime;

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
        m_lastPackageTime = DateTime.Now;
    }

    public void ShootAtDirection(Vector2 direction) {
        Network.Log("Player " + Id + " shot at direction " + direction);
        SimpleRay2D ray = new SimpleRay2D();
        ray.origin = m_position;
        ray.direction = direction;
        ray.length = m_laserLength;
        world.ShootLaser(ray, this);
    }

    public float ReceiveDamage( float dmg )
    {
        float powerIncome = 0.0f;

        Network.Log("Player " + Id + " receives " + dmg + " damage.");

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
            if (m_power < 0.01f)
                Die();
        }

        Network.Log("Player " + Id + " now has " + m_power + " power.");

        powerIncome *= Mathf.Exp( -dmg / 15 );

        

        return powerIncome;
    }

    public void Die() {
        m_power = 0;
        IsAlive = false;
        m_lastDeathTime = DateTime.Now;

        Network.Log("Player " + Id + " died.");

        if (OnDeath != null)
            OnDeath(this);
    }

    public void Respawn(Vector2 position) {
        IsAlive = true;
        m_position = position;
        m_power = 1;
    }

    public float GetPower() { return m_power; }

    public float GetLaserDamage()
    {
        return m_power*0.5f;
    }

    public bool Intersects( SimpleRay2D ray )
    {
        Vector2 L = m_position - ray.origin;
        float tca = Vector2.Dot( L, ray.direction );

        if( tca < 0 )
            return false;

        float d2 = Vector2.Dot( L, L ) - tca * tca;
        if ( d2 > Radius * Radius )
            return false;

        float thc = Radius - Mathf.Sqrt( d2 );
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
        m_lastPackageTime = DateTime.Now;
        Id = id;
    }
}

/// <summary>
/// Data for a simple ray in the game's World.
/// </summary>
public class SimpleRay2D
{
    public Vector2 origin;
    public Vector2 direction;
    public float length;
}

/// <summary>
/// The entire gameplay on the server happens in here. It contains all the player references, listens to
/// adequate events, and sends PlayerStates and other events to all the players.
/// </summary>
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
                //Console.WriteLine("InputEvent: Target pawn doesn't exist");
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

    public class ProperShotListener : ReliableEventListener, IEventListener {
        World m_world = null;
        public ProperShotListener(World w) {
            m_world = w;
        }

        public bool Execute(EventBase e) {
            ShotEvent shot = (ShotEvent)e;

            Network.Server.RespondToReliableEvent(shot.m_reliableEventId, shot.m_who);

            if (!WasExecuted(shot.m_who, shot.m_reliableEventId))
            {
                PlayerPawn pawn = m_world.TryGetPawn(shot.m_who);
                //Console.WriteLine("Looking for pawn with id " + input.m_sessionId + ": " + pawn);
                if (pawn != null && pawn.m_isPlayingNow)
                {
                    pawn.ShootAtDirection(shot.m_direction);
                }

                AddExecuted(shot.m_who, shot.m_reliableEventId);
                return true;
            }
            else return false;
        }

        public EventType GetEventType() {
            return (EventType)ShotEvent.GetStaticId();
        }
    }

    Dictionary< int, PlayerPawn > m_players 
        = new Dictionary<int, PlayerPawn>();

    public int respawnTime = 5;

    ProperInputListener m_inputListener = null;// new ProperInputListener();
    ProperPleaseSpawnListener m_spawnListener = null;
    private ProperShotListener m_shotListener = null;

    System.Random m_radom = new System.Random();

    Thread updateThread;

    public void Init()
    {
        m_inputListener = new ProperInputListener(this);
        m_spawnListener = new ProperPleaseSpawnListener(this);
        m_shotListener = new ProperShotListener(this);
        Network.AddListener(m_inputListener);
        Network.AddListener(m_spawnListener);
        Network.AddListener(m_shotListener);

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

            RespawnLoop();
        }
    }

    private PlayerPawn TryGetPawn(int id) {
        PlayerPawn pawn = null;
        m_players.TryGetValue(id, out pawn);
        return pawn;
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

    public void RespawnLoop()
    {
        var dead = m_players.Where(x => !x.Value.IsAlive).ToList();
        foreach (var kvp in m_players.Where(x=>!x.Value.IsAlive)) {
            if (kvp.Value.m_isPlayingNow
                && (DateTime.Now - kvp.Value.m_lastDeathTime).Seconds >= respawnTime)
            {
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
        Network.Log("Laser hits " + playersHit.Count + " players.");
        float laserPower = owner.GetLaserDamage();
        float powerAccumulator = 0.0f;
        for ( int i = 0; i < playersHit.Count; ++i )
        {
            if ( playersHit[i] == owner )
                continue;

            powerAccumulator += playersHit[i].ReceiveDamage( laserPower );
        }

        owner.AddPower( powerAccumulator );

        ShotEvent se = new ShotEvent();
        se.m_direction = ray.direction;
        se.m_point = owner.Position;
        se.m_who = owner.Id;
        se.m_reliableEventId = Network.Server.GetNewReliableEventId();
        foreach (int id in m_players.Keys)
        {
            Network.Server.Send(se, id, true);
        }

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

    public List<int> GetPlayersWithNoNewPackages(int secondsThreshold)
    {
        DateTime now = DateTime.Now;
        List<int> result = new List<int>();
        foreach (KeyValuePair<int, PlayerPawn> pair in m_players)
        {
            if ((now - pair.Value.m_lastPackageTime).Seconds > secondsThreshold)
            {
                result.Add(pair.Key);
            }
        }
        return result;
    }
}