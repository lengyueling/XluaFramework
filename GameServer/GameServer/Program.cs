using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start(8000,2000);
            ServerTest.Instance.Init();
            Console.ReadKey();
        }
    }
}
