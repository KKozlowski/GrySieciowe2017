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

    public System.Action<PlayerConnection, int> OnConnect;

    public void Awake()
    {
        Network.Log = Debug.Log;
        //StartCoroutine(CoroutineConnect("127.0.0.1", 966, 1337));
    }

    private void Update() {
        if (Network.Client != null && Network.Client.ConnectionId >= 0)
        {
            Network.Client.ResendRemainingReliables();
        }
    }

    public void OnDestroy()
    {
        if (Network.Client!=null)
            Network.Client.Shutdown();
    }

    public void Connect(string serverIp, int myPort, int serverPort, System.Action callback=null)
    {
        StartCoroutine(CoroutineConnect(serverIp, myPort, serverPort, callback));
    }

    IEnumerator CoroutineConnect(string serverIp, int myPort, int serverPort, System.Action callback=null) {
        Debug.Log("Connection try started");
        Network.InitAsClient(serverIp, myPort, serverPort);
        while (Network.Client.ConnectionId < 0)
            yield return null;
        yield return null;
        if (callback != null)
            callback();
        if (OnConnect != null)
            OnConnect(this, Network.Client.ConnectionId);
        Network.AddListener(new RespawnListener());
        Debug.Log("Connected");

        yield return new WaitForSeconds(1.0f);
        
        {
            var e = new SpawnRequestEvent(Network.Client.ConnectionId);

            Network.Client.Send(e);
        }
    }
}
