using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UDPTestClient
{
    public partial class Form1 : Form
    {
        UDPTalkChannel.Client client;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            testMessage tstMsg = new testMessage()
            {
                userName=txtUsername.Text,
                ID=Convert.ToInt32(txtID.Text),
                textMessage=txtMessage.Text
            };
            client.Send(tstMsg.ToByte());
        }

        void client_onReceived(object sender, UDPTalkChannel.Client.ReceivedEventArgs e)
        {
            testMessage tstMsg = new testMessage();
            tstMsg = (testMessage)tstMsg.ToMessage(e.MessageByte);
     
            string strTem = string.Format("Received: {0}, from: {1}, ID: {2}, IP: {3}", tstMsg.textMessage, tstMsg.userName, tstMsg.ID, e.senderClient.ToString());

            MessageBox.Show(strTem);
        }
        void client_onError(object sender, UDPTalkChannel.Client.ErrorEventArgs e)
        {
            MessageBox.Show(e.Ex.Message, "Error");
        }
        void client_onSent(object sender, UDPTalkChannel.Client.SentEventArgs e)
        {
            MessageBox.Show("Message Sent");
        }
        private void btnInit_Click(object sender, EventArgs e)
        {
            client = new UDPTalkChannel.Client(txtRemoteIP.Text, Convert.ToInt32(txtOutPort.Text), Convert.ToInt32(txtInPort.Text), Convert.ToInt32(txtBufferSize.Text));
            client.onSent += new UDPTalkChannel.Client.SentEventHandler(client_onSent);
            client.onError += new UDPTalkChannel.Client.ErrorEventHandler(client_onError);
            client.onReceived += new UDPTalkChannel.Client.ReceivedEventHandler(client_onReceived);
        }
    }
}
