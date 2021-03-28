using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public partial class Form1 : Form
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientSockets = new List<Socket>();

        bool listening = false;
        bool terminating = false;
  

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(form1FormClosing);
            InitializeComponent();
        }

        // TERMINATE WHEN THE PROGRAM CLOSES
        private void form1FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void accept()
        {
            while(listening)
            {
                try
                {
                    // CREATE A NEW CLIENT AND STORE IT IN THE LIST CREATED
                    Socket newClient = serverSocket.Accept();
                    clientSockets.Add(newClient);
                    TextBoxLog.AppendText("A client is connected.\n");

                    Thread recieveThread = new Thread(() => Recieve(newClient));
                    recieveThread.Start();
                }
                catch
                {
                    if(terminating)
                    {
                        listening = false;
                    }
                   else
                    {
                        TextBoxLog.AppendText("Unable to connect.");
                    }
                }
            }
        }

        private void Recieve(Socket thisClient)
        {
            bool connected = true;

            while (connected  && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    thisClient.Receive(buffer);

                    String incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
                    TextBoxLog.AppendText("Client: " + incomingMessage + "\n");
                }
                catch
                {
                    if (terminating)
                    {
                        TextBoxLog.AppendText("A client has disconnected.\n");

                    }
                    thisClient.Close();
                    clientSockets.Remove(thisClient);
                    connected = false;
                }
            }
        }

        private void buttonListen_Click(object sender, EventArgs e)
        {
            int port;

            if(Int32.TryParse(textBoxPort.Text, out port))
            {
                // CREATE END POINT FOR THE CONNECTION
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(3);

                listening = true;
                buttonListen.Enabled = false;

                // ENABLE THE MESSAGE SENDING OPTION WHEN CONNCETION IS ESATBLISHED
                textBoxMsg.Enabled = true;
                buttonSend.Enabled = true;

                Thread acceptThread = new Thread(accept);
                acceptThread.Start();
            }
            else
            {

            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            string message = textBoxMsg.Text;

            if(message != "" && message.Length <= 64)
            {
                Byte[] buffer = Encoding.Default.GetBytes(message);
                foreach(Socket client in clientSockets)
                {
                    try
                    {
                        client.Send(buffer);
                    }
                    catch
                    {
                        TextBoxLog.AppendText("There is a problem! Check the connection.\n");
                        terminating = true;
                        textBoxMsg.Enabled = false;
                        buttonSend.Enabled = false;
                        textBoxPort.Enabled = true;
                        buttonListen.Enabled = true;
                        serverSocket.Close();
                    }
                }
            }
        }
    }
}
