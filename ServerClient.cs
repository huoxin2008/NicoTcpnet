using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Nico.Tcpnet
{
    public class ServerClient
    {
        NetServer server;
        public string Name;
        public string Tag;

        public int Id { get{return id;}  }
        public TcpClient tcpClient { get { return client; } }

        int id;
        TcpClient client;
        /*     BinaryReader br;
             BinaryWriter bw;*/
        NetworkStream ns;
        byte[] data;
       const  int PACKLEN = 1024;

        public ServerClient(TcpClient c , NetServer s)
        {
            server = s;
            client = c;
            id = Guid.NewGuid().GetHashCode();

            ns=client.GetStream();
            /*  br = new BinaryReader(ns);
            bw = new BinaryWriter(ns);*/

            data = new byte[4];
             
             ns.BeginRead(data, 0, data.Length,  ReadLen ,null);
        }

        void ReadLen(IAsyncResult ar)
        {
            try
            {
                int len = ns.EndRead(ar);
                if (len == 4)
                {
                    var datalen = BitConverter.ToInt32(data, 0);
                    data = new byte[datalen];
                    ns.BeginRead(data, 0, data.Length, ReadData, null);
                }
            }
            catch {
                ns.Close();
                tcpClient.Close();
                server.Remove(this);
            }
          
        }
        void ReadData(IAsyncResult ar)
        {
            try
            {
                int len = ns.EndRead(ar);
                if (len == data.Length )
                {
                  //处理数据
                    server.processData(this, data);
                    data = new byte[4];
                    ns.BeginRead(data, 0, data.Length, ReadLen, null);
                }
            }
            catch
            {
                ns.Close();
                tcpClient.Close();
                server.Remove(this);
            }
        }


        public void Send(object msg)
        {
            var d = JsonSeriHelp.Serialize(msg);

            var tn = msg.GetType().FullName;
            if (tn != null)
            {
                var tndata = Encoding.UTF8.GetBytes(tn);
                var tnlen = BitConverter.GetBytes(tndata.Length);

                var data = tnlen.Concat(tndata).Concat(d).ToArray();
                var datalen = BitConverter.GetBytes(data.Length);
                var alldata = datalen.Concat(data).ToArray();

               ns.BeginWrite(alldata, 0, alldata.Length , a => {
                    var v = a.IsCompleted;
                }, null);
            }
           
        }
        

    }
}
