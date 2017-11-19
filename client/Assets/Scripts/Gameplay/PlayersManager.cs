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
            Debug.Log("STATES");
            PlayerStateEvent ps = (PlayerStateEvent)e;

            m_manager.ApplyState(ps.state);

            return true;
        }

        public EventType GetEventType() {
            return (EventType)InputEvent.GetStaticId();
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

    private Dictionary<int, PlayerInstanceState> states 
        = new Dictionary<int, PlayerInstanceState>();

    public void ApplyState(PlayerState ps) {
        Debug.Log("Apply state");
        PlayerInstanceState pis = null;
        if (!states.TryGetValue(ps.id, out pis)) {
            pis = new PlayerInstanceState();
            pis.state = ps;
            pis.controller = Instantiate(characterPrefab);
            pis.controller.Init(ps.id == Network.Client.ConnectionId);
            states[ps.id] = pis;
        }

        pis.controller.MoveTo(pis.state.position);
    }
}
