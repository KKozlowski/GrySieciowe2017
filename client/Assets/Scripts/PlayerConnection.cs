using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConnection : MonoBehaviour
{
    public class StupidInputListener : IEventListener {
        System.Action OnRespawn = null;

        public bool Execute(EventBase e) {
            InputEvent input = (InputEvent)e;
            Debug.Log("Received input event for some reason");

            return false;
        }

        public EventType GetEventType() {
            return (EventType)InputEvent.GetStaticId();
        }
    }

    public class RespawnListener : IEventListener {
        System.Action OnRespawn = null;

        public bool Execute(EventBase e) {
            SpawnRequestEvent respawn = (SpawnRequestEvent)e;
            Debug.Log("Respawned?");
            if (respawn.m_sessionId == Network.Client.ConnectionId) {
                Debug.Log("RESPAWNED INDEED");
            }

            return true;
        }

        public EventType GetEventType() {
            return (EventType)SpawnRequestEvent.GetStaticId();
        }
    }

    public void Awake()
    {
        Network.Log = (object o) => { Debug.Log(o); };
        StartCoroutine(Connect("127.0.0.1", 966, 1337));
    }

    private void Start() {
        Network.AddListener(new RespawnListener());
        Network.AddListener(new StupidInputListener());
    }

    public void OnDestroy()
    {
        Network.Client.Shutdown();
    }

    IEnumerator Connect(string serverIp, int myPort, int serverPort) {
        Debug.Log("Connection try started");
        Network.InitAsClient(serverIp, myPort, serverPort);
        while (Network.Client.ConnectionId < 0)
            yield return null;
        yield return new WaitForSeconds(2.0f);
        Debug.Log("Connected");
        {
            var e = new SpawnRequestEvent(Network.Client.ConnectionId);

            Network.Client.Send(e);
        }
        
        {
            InputEvent e = new InputEvent();
            e.m_sessionId = 0;
            e.m_direction = new Vector2(1.2f, 3.4f);
            Network.Client.Send(e);
        }
    }
}
