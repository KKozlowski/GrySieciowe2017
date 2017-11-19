using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConnection : MonoBehaviour
{
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

    public void Start()
    {
        Network.Log = (object o) => { Debug.Log(o); };
        StartCoroutine(Connect());
    }

    public void OnDestroy()
    {

    }

    IEnumerator Connect() {
        Debug.Log("DD1");
        Network.InitAsClient("127.0.0.1", 966, 1337, this);
        yield return new WaitForSecondsRealtime(1.0f);
        Debug.Log("DD");
        var e = new SpawnRequestEvent(Network.Client.ConnectionId);
        Network.Client.Send(e);
    }
}
