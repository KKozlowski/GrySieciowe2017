using System.Threading;
using UnityEngine;

namespace ClientApp
{
    class Program
    {
        static void Main( string[] args )
        {
            Network.Init( false );

            Thread.Sleep( 1000 );
            InputEvent e = new InputEvent();
            e.m_direction = new Vector2( 1.2f, 3.4f );
            Network.Client.Send( e );

            while ( true )
            {
                Thread.Sleep( 50 );
            }
        }
    }
}
