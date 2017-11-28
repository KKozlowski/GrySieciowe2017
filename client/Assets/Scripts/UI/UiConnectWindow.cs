using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class UiConnectWindow : MonoBehaviour
{
    [SerializeField] private Text yourIpText;
    [SerializeField] private InputField serverIp, serverPort, clientPort;

    [SerializeField] private PlayerConnection connection;

    public static UiConnectWindow Me { get; private set; }

	// Use this for initialization
	void Start ()
	{
	    yourIpText.text = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
	    Me = this;
	}

    public void Show()
    {
        gameObject.SetActive(true);   
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Connect()
    {
        string sIp = serverIp.text;
        int sPort = -1;
        int myPort = -1;
        if (!int.TryParse(serverPort.text, out sPort))
        {
            Debug.Log("Failed server port parse");
            return;
        }
        if (!int.TryParse(clientPort.text, out myPort))
        {
            Debug.Log("Failed client port parse");
            return;
        }

        Debug.Log("Connection start");
        connection.Connect(sIp, myPort, sPort, Hide);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
