using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerApp {
    class Program {
        static void Main(string[] args) {
            NetServer srv = new NetServer();
            srv.Start( 1111 );

            {
                PlayerState ps1 = new PlayerState();
                ps1.position = new Vector2(300, 400);
                ps1.id = 21;
                ps1.health = 100;

                ByteStreamWriter msg = new ByteStreamWriter();
                ps1.Serialize( msg );

                PlayerState ps2 = new PlayerState();
                ps2.id = 21;
                ps2.Deserialize(new ByteStreamReader(msg));

                Console.WriteLine("ps2 position: " + ps2.position);
                Console.WriteLine("ps2 health: " + ps2.health);
            }

            while( true )
            {

            }
        }
    }
}
