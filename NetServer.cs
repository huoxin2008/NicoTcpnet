using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Nico.Tcpnet
{
    public  class  NetServer
    {
        TcpListener listen;
        Dictionary<int, ServerClient> clients = new Dictionary<int, ServerClient>();
        public ServerClient this[int index]{
            get { return clients.Values.ToList()[index]; }
        }
        public event ConnectHandle Event_Connect;
        public event QuickHandle Event_Quit;
        Dictionary<Type, Delegate> RecvData = new Dictionary<Type, Delegate>();
        public NetServer(int port , System.Windows.Forms.Control control=null)
        {
            if (control != null)
                TypeFactory.Control = control;
            listen = new TcpListener(port);
            listen.Start();
            listen.BeginAcceptTcpClient(RecvConect, null);
        }

        void RecvConect(IAsyncResult a)
        {
            var client = listen.EndAcceptTcpClient(a);
            var c = new ServerClient(client , this);
            clients.Add(c.Id,c);
            if (Event_Connect != null)
            {
                Event_Connect.BeginInvoke(c.Id,c,null,null);
            }
            listen.BeginAcceptTcpClient(RecvConect, null); 
        }

        internal void processData(ServerClient c, byte[] data)
        {
               int tnlen= BitConverter.ToInt32(data, 0);
               var tname=Encoding.UTF8.GetString(data, 4, tnlen);
               var t=TypeFactory.GetType(tname);
               if (t != null)
               {
                  var rcvdata= JsonSeriHelp.Deserialize(t, data, 4+tnlen );
                       if (RecvData.ContainsKey(t))
                       {
                           var action = new Action(() =>
                           {
                               foreach (var a in RecvData[t].GetInvocationList())
                                   a.DynamicInvoke(c, rcvdata);
                           });
                           if (TypeFactory.Control != null)
                               TypeFactory.Control.BeginInvoke(action);
                           else 
                             action.BeginInvoke(null, null);

                       }
               }
        }

        public void Send(ServerClient c, object msg)
        {
            c.Send(msg);
        }

        public void Remove(ServerClient c)
        {
            clients.Remove(c.Id);
            if (Event_Quit != null)
                Event_Quit(c.Id,c);
        }

        public void RegistRecvHandle<T>(Action<ServerClient,T> func )
        {
            var type=typeof(T);
            if (RecvData.ContainsKey(type) == false)
            {
                TypeFactory.RegistType<T>();
                RecvData.Add( type, func);
                
            }
            else
            {
                RecvData[type] = Delegate.Combine(RecvData[type],func);
            }
        }

    }

   public delegate void QuickHandle(int clientid, ServerClient client);
   public delegate void ConnectHandle(int clientid, ServerClient client);
   public delegate void RecvHandle( ServerClient client , object msg );
}
