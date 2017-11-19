using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerApp {
    class Program {
        public class TestInputLstener : IEventListener
        {
            public bool Execute( EventBase e )
            {
                InputEvent input = (InputEvent)e;
                Console.WriteLine( "Dupa: " + input.m_direction.ToString() );
                return false;
            }

            public EventType GetEventType()
            {
                return (EventType)InputEvent.GetStaticId();
            }
        }

        static void Main(string[] args)
        {
            Network.Init( true );
            Network.AddListener( new TestInputLstener() );

            {
                PlayerState ps1 = new PlayerState();
                ps1.position = new Vector2(300, 400);
                ps1.id = 21;
                ps1.power = 100;

                ByteStreamWriter msg = new ByteStreamWriter();
                ps1.SetHealthDirty(true);
                ps1.SetPositionDirty(true);
                ps1.Serialize( msg );

                PlayerState ps2 = new PlayerState();
                ps2.id = 21;
                ps2.Deserialize(new ByteStreamReader(msg));

                Console.WriteLine("ps2 position: " + ps2.position);
                Console.WriteLine("ps2 power: " + ps2.power);
            }

            while( true )
            {
                Thread.Sleep(50);
            }
        }
    }
}
