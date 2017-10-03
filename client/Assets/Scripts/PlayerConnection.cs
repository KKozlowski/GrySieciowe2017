using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConnection : MonoBehaviour
{
    Client m_client;
    public void Start()
    {
        m_client = new Client();
        m_client.InitConnection( "127.0.0.1", 1111 );
    }

}
