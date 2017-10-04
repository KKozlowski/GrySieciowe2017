using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerApp {
    class Program {
        static void Main(string[] args) {
            Server srv = new Server();
            srv.Start( 1111 );
            Vector3 v = new Vector3(0, 0, 0);
            while( true )
            {

            }
        }
    }
}
