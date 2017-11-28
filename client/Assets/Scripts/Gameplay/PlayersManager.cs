using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour {
    public class StateListener : IEventListener {
        PlayersManager m_manager = null;
        public StateListener(PlayersManager w) {
            m_manager = w;
        }

        public bool Execute(EventBase e) {
            PlayerStateEvent ps = (PlayerStateEvent)e;
            //Debug.Log("ID: " + ps.state.id + ", position: "+ ps.state.position);
            
            foreach (PlayerState state in ps.states)
            {
                m_manager.EnqueueState(state);
            }
            return true;
        }

        public EventType GetEventType() {
            return (EventType)PlayerStateEvent.GetStaticId();
        }
    }

    public class ShotListener : ReliableEventListener, IEventListener {
        PlayersManager m_manager = null;
        public ShotListener(PlayersManager w) {
            m_manager = w;
        }

        public bool Execute(EventBase e) {
            
            ShotEvent ps = (ShotEvent)e;
            Network.Client.RespondToReliableEvent(ps.m_reliableEventId);
            Debug.Log("Received reliable event: " + ps.m_reliableEventId);

            if (WasExecuted(0, ps.m_reliableEventId))
                return false;

            PlayerInstanceState pis = null;
            m_manager.playerInstances.TryGetValue(ps.m_who, out pis);
            if (pis!=null)
            {
                m_manager.shotDatas.Enqueue(new ShotData()
                {
                    controller = pis.controller,
                    direction = ps.m_direction,
                    point =  ps.m_point
                });
            }

            AddExecuted(0, ps.m_reliableEventId);
            return true;
        }

        public EventType GetEventType() {
            return (EventType)ShotEvent.GetStaticId();
        }
    }

    private class ShotData
    {
        public CharacterController controller;
        public Vector2 point;
        public Vector2 direction;
    }

    [SerializeField]
    private CharacterController characterPrefab;

    [SerializeField] private PlayerConnection connection;

    private StateListener m_listenerOfStates;
    private ShotListener m_shotListener;

    private void Start()
    {
        connection.OnConnect += (PlayerConnection playerConnection, int connectionId) =>
        {
            Init();
        };
    }

    private bool initialized = false;
    public void Init()
    {
        if (!initialized)
        {
            m_listenerOfStates = new StateListener(this);
            m_shotListener = new ShotListener(this);
            Network.AddListener(m_listenerOfStates);
            Network.AddListener(m_shotListener);
            initialized = true;
        }
        
    }

    public class PlayerInstanceState {
        public PlayerState state;

        public CharacterController controller;
        public DateTime lastUpdateTime;
    }

    private Dictionary<int, PlayerInstanceState> playerInstances 
        = new Dictionary<int, PlayerInstanceState>();

    private Queue<PlayerState> statesToApply = new Queue<PlayerState>();
    private Queue<ShotData> shotDatas = new Queue<ShotData>();

    public void EnqueueState(PlayerState ps) {
        //Debug.Log("Apply state for " + ps.id + "(pos: "+ps.position+")");

        statesToApply.Enqueue(ps);
    }

    void Update() {
        while(statesToApply.Count > 0) {
            ApplyState(statesToApply.Dequeue());
        }
        while (shotDatas.Count > 0)
        {
            var shot = shotDatas.Dequeue();
            shot.controller.MoveTo(shot.point);
            shot.controller.Shoot(shot.direction);
        }

        bool playerRemoved = false;
        DateTime now = DateTime.Now;
        List<PlayerInstanceState> toRemove = new List<PlayerInstanceState>();
        foreach (var pis in playerInstances)
        {
            if ((now - pis.Value.lastUpdateTime).Seconds > 2)
                toRemove.Add(pis.Value);
        }
        foreach (PlayerInstanceState i in toRemove)
        {
            Debug.Log("Removed: " + i.controller.name);
            if (i.controller.IsPlayer)
                playerRemoved = true;
            Destroy(i.controller.gameObject);
            playerInstances.Remove(i.state.id);
        }

        if (playerRemoved)
        {
            Network.Client.Shutdown();
            UiConnectWindow.Me.Show();
        }
    }

    public void ApplyState(PlayerState ps) {
        if (ps == null)
            return;
        ;
        PlayerInstanceState pis = null;
        if (!playerInstances.TryGetValue(ps.id, out pis)) {
            pis = new PlayerInstanceState();
            pis.controller = Instantiate(characterPrefab);
            pis.controller.Init(ps.id == Network.Client.ConnectionId);
            playerInstances[ps.id] = pis;
        }

        pis.state = ps;
        pis.controller.MoveTo(pis.state.position);
        pis.controller.Power = pis.state.power;
        pis.lastUpdateTime = DateTime.Now;
    }
}
