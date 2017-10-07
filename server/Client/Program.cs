using System;
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
            Client client = new Client();
            client.Connect( "127.0.0.1", 1111 );
            
            while ( true )
            {
                Thread.Sleep( 50 );
            }
        }
    }
}
