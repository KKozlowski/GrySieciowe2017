﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {
        static void Main( string[] args )
        {
            NetClient client = new NetClient();
            MessageDispatcher dispatcher = new MessageDispatcher();
            MessageDeserializer deserializer = new MessageDeserializer( dispatcher );
            client.SetDeserializer(deserializer);
            deserializer.connectionMessagesReceiver = client;

            client.Connect( "127.0.0.1", 1111 );
            Console.WriteLine("DD");
            
            while ( true )
            {
                Thread.Sleep( 50 );
            }
        }
    }
}
