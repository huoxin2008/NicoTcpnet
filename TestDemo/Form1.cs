using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nico.Tcpnet;

namespace TestDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var ac = new Action(() => { int c = 0;
            
            });
            ac.Invoke();
            var dd = ac.Target;
        }

        NetServer server;

        private void btnStart_Click(object sender, EventArgs e)
        {
            server = new NetServer(int.Parse(textBox1.Text),this);
            server.RegistRecvHandle<Msg>((c, Msg) =>
            {
                textBox2.AppendText(Msg.name + ":" + Msg.time.ToLongTimeString() + Environment.NewLine + Msg.text + Environment.NewLine);
            });
            btnStart.Enabled = false;
        }

        private void btnSendServer_Click(object sender, EventArgs e)
        {
           
            server.Send(server[0],new Msg { name = "Server", text = textBox4.Text  , time=DateTime.Now} );
        }
        NetClient client;
        private void btnStartClient_Click(object sender, EventArgs e)
        {
            client = new NetClient("127.0.0.1", int.Parse(textBox1.Text) );
            client.RegistRecvHandle<Msg>(Msg=>{
                textBox3.AppendText(Msg.name + ":" + Msg.time.ToLongTimeString() + Environment.NewLine + Msg.text + Environment.NewLine);
            });
            client.StartRecv();
            btnStartClient.Enabled = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            client.Send(new Msg { name = "Client", text = textBox5.Text  , time=DateTime.Now});
        }

        
    }
}
