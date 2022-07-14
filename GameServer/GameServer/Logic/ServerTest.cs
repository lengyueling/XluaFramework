using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GameServer
{
    public class ServerTest : Singleton<ServerTest>
    {
        public void Init()
        {
            MessageRegister.Instance.Subscribe<Test>(MessageID.Test, OnTest);
            MessageRegister.Instance.Subscribe<Login>(MessageID.Login, OnLogin);
        }

        private void OnTest(Connection sender, Test test)
        {
            foreach (var item in test.listTest)
            {
                Console.WriteLine(item);
            }


            TestRes testRes = new TestRes();
            testRes.v3 = new Vector3(1,1,1);
            testRes.id = 9999;
            testRes.listTest = new List<int>();
            testRes.listTest.Add(4);
            testRes.listTest.Add(5);
            testRes.listTest.Add(6);
            testRes.user = "---------";
            testRes.password = "***********";

            sender.Send(testRes);
        }

        public void OnLogin(Connection sender, Login test)
        {
            LoginRes res = new LoginRes();
            sender.Send(res);
        }
    }
}