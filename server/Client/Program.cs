using System.Threading;
using UnityEngine;

namespace ClientApp
{
    class Program
    {
        static void Main( string[] args )
        {
            Network.InitAsClient("127.0.0.1", 1000, 1337);

            Thread.Sleep( 1000 );
            InputEvent e = new InputEvent();
            e.m_sessionId = 0;
            e.m_direction = new Vector2( 1.2f, 3.4f );
            Network.Client.Send( e );

            while ( true )
            {
                Thread.Sleep( 50 );
            }
        }
    }
}
