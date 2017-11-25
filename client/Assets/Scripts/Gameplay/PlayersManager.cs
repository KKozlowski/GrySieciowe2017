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

    [SerializeField]
    private CharacterController characterPrefab;

    private StateListener m_listenerOfStates;

    private void Start() {
        m_listenerOfStates = new StateListener(this);
        Network.AddListener(m_listenerOfStates);
    }


    public class PlayerInstanceState {
        public PlayerState state;

        public CharacterController controller;
    }

    private Dictionary<int, PlayerInstanceState> playerInstances 
        = new Dictionary<int, PlayerInstanceState>();

    private Queue<PlayerState> statesToApply = new Queue<PlayerState>();

    public void EnqueueState(PlayerState ps) {
        Debug.Log("Apply state for " + ps.id + "(pos: "+ps.position+")");

        statesToApply.Enqueue(ps);
    }

    void Update() {
        while(statesToApply.Count > 0) {
            ApplyState(statesToApply.Dequeue());
        }
    }

    public void ApplyState(PlayerState ps) {
        PlayerInstanceState pis = null;
        if (!playerInstances.TryGetValue(ps.id, out pis)) {
            pis = new PlayerInstanceState();
            pis.controller = Instantiate(characterPrefab);
            pis.controller.Init(ps.id == Network.Client.ConnectionId);
            playerInstances[ps.id] = pis;
        }

        pis.state = ps;
        pis.controller.MoveTo(pis.state.position);
    }
}
