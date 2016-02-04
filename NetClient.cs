using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Nico.Tcpnet
{
    public class NetClient
    {

        public TcpClient tcpClient;
        public event ConnectSuccess Event_ConnectSuccess;
        Dictionary<Type, Delegate> RecvData = new Dictionary<Type, Delegate>();
         //Dictionary<Type, Type> Types = new Dictionary<Type, Type>();

        public bool IsConnect { 
            get {
           return  tcpClient==null?false :tcpClient.Connected ;
          }
        }

        NetworkStream ns;
        byte[] data;

        public NetClient(string ip, int port, System.Windows.Forms.Control control = null)
        {
            if (control != null)
                TypeFactory.Control = control;
            tcpClient = new TcpClient();
            tcpClient.BeginConnect(IPAddress.Parse(ip), port,a=>{
               
                StartRecv();
                if (Event_ConnectSuccess != null)
                {
                    Event_ConnectSuccess();
                }
              /*  if (isAutoRecv && tcpClient.Connected)
                {
                    data = new byte[4];
                    ns.BeginRead(data, 0, data.Length, ReadLen, null);
                }*/
            },null );
            

        }

        public void StartRecv()
        {
            if ( tcpClient.Connected)
            {
                ns = tcpClient.GetStream();
                data = new byte[4];
                ns.BeginRead(data, 0, data.Length, ReadLen, null);
            }
        }

        void ReadLen(IAsyncResult ar)
        {
            try { 
                int len = ns.EndRead(ar);
                if (len == 4)
                {
                    var datalen = BitConverter.ToInt32(data, 0);
                    data = new byte[datalen];
                    ns.BeginRead(data, 0, data.Length, ReadData, null);
                }
            }
            catch
            {
                ns.Close();
                tcpClient.Close();
            }
        }
        void ReadData(IAsyncResult ar)
        {
            try
            {
                int len = ns.EndRead(ar);
                if (len == data.Length)
                {
                    int tnlen = BitConverter.ToInt32(data, 0);
                    var tname = Encoding.UTF8.GetString(data, 4, tnlen);
                    var t = TypeFactory.GetType(tname);
                    if (t != null)
                    {
                        var rcvdata = JsonSeriHelp.Deserialize(t, data, 4 + tnlen);
                        if (RecvData.ContainsKey(t)  )
                        {
                           var action= new Action(() => {
                            foreach (var b in RecvData[t].GetInvocationList())
                            {
                                b.DynamicInvoke(rcvdata);
                            }
                            });
                           if (TypeFactory.Control != null)
                               TypeFactory.Control.BeginInvoke(action);
                           else 
                             action.BeginInvoke(null, null);
                        }
                    }
                    //处理数据
                    data = new byte[4];
                    ns.BeginRead(data, 0, data.Length, ReadLen, null);
                }
            }  
            catch
            {
                ns.Close();
                tcpClient.Close();
            }
        }

        public void Send(object msg)
        {
            var d = JsonSeriHelp.Serialize(msg);
           
            var tn = msg.GetType().FullName;;
            if (tn != null)
            {
                var tndata = Encoding.UTF8.GetBytes(tn);
                var tnlen = BitConverter.GetBytes(tndata.Length);

                var data = tnlen.Concat(tndata).Concat(d).ToArray();
                var datalen = BitConverter.GetBytes(data.Length);
                var alldata = datalen.Concat(data).ToArray();

                ns.BeginWrite(alldata, 0, alldata.Length, null, null);

            }
        }

        public void RegistRecvHandle<T>(Action<T> func)
        {
           var type = typeof(T);
           if (RecvData.ContainsKey(type) == false)
           {
               TypeFactory.RegistType<T>();
               RecvData.Add(type, func);
           }
           else
           {
               RecvData[type] = Delegate.Combine(RecvData[type], func);
           }
        }

        
    }
  
    public delegate void ConnectSuccess();
}
