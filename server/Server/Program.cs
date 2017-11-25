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

            Console.WriteLine("SERVER APP Launched.\n==========");

            while( true )
            {
                Thread.Sleep(50);
            }
        }
    }
}
