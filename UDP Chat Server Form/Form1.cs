﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UDP_Chat_Server_Form
{
    public partial class Form1 : Form
    {
        UDP_Asynchronous_Chat.UDPAshynchronousChatServer mUDPChatServer;

        public Form1()
        {
            mUDPChatServer = new UDP_Asynchronous_Chat.UDPAshynchronousChatServer(); 

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mUDPChatServer.startReceivingData();
        }
    }
}
