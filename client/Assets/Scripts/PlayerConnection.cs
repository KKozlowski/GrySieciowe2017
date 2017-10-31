using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerConnection : MonoBehaviour
{
    NetClient m_client;
    Thread m_connectThread;

    public void Start()
    {
        m_client = new NetClient();
        m_connectThread = new Thread( () => m_client.Connect( "127.0.0.1", 1111 ) );
        m_connectThread.Start();
    }

    public void OnDestroy()
    {
        m_connectThread.Interrupt();
        m_client.Shutdown();
    }
}
