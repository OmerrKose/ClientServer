using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(form1FormClosing);
            InitializeComponent();
        }

        private void form1FormClosing(object sender, EventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void Recieve()
        {
            while(connected)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    clientSocket.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
                    richTextBoxLog.AppendText("Server: " + incomingMessage + "\n");  
                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBoxLog.AppendText("The server is disconnected.");
                        buttonConnect.Enabled = true;
                        textBoxMsg.Enabled = false;
                        buttonSend.Enabled = false;
                    }
                    clientSocket.Close();
                    connected = false;
                }
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = textBoxIp.Text;
            int portNumber;

            if(Int32.TryParse(textBoxPort.Text, out portNumber))
            {
                try
                {
                    clientSocket.Connect(IP, portNumber);
                    buttonConnect.Enabled = false;
                    textBoxMsg.Enabled = true;
                    buttonSend.Enabled = true;
                    connected = true;
                    richTextBoxLog.AppendText("Connected to the server.\n");

                    Thread recieveThread = new Thread(Recieve);
                    recieveThread.Start();
                }
                catch
                {
                    richTextBoxLog.AppendText("Could not connect to the server. \n");
                }    
            }
            else
            {
                textBoxMsg.AppendText("Check the port.\n");
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            string message = textBoxMsg.Text;

            if (message != "" && message.Length <= 64)
            {
                Byte[] buffer = new Byte[64];
                buffer = Encoding.Default.GetBytes(message);
                clientSocket.Send(buffer);
            }
            else
            {

            }
        }
    }
}
