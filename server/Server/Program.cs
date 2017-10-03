using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp {
    class Program {
        static void Main(string[] args) {
            Server srv = new Server();
            srv.Start( 1111 );

            while( true )
            {

            }
        }
    }
}
