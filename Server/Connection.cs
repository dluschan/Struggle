﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Struggle
{
    class Connection
    {
        public Socket socket
        {
            get;
            private set;
        }
        public int id
        {
            private set;
            get;
        }
        public string nick
        {
            private set;
            get;
        }

        ConcurrentDictionary<int, Connection> refConnections;
        IdMap refIds;

        public bool timeoutFlag;
        public Timer timeoutTimer;

        public byte[] data;



        public int fraction { get; private set; }

        /*Entities stuff*/




        public Connection(Socket socket, int id, int fraction)
        {
            timeoutFlag = true;
            this.socket = socket;
            this.id = id;
            this.fraction = fraction;
            data = new byte[128];
            nick = "Player";
        }

        public void SetFraction(int fraction)
        {
            this.fraction = fraction;
        }

        public void SetupTimeout(ref ConcurrentDictionary<int, Connection> connections, ref IdMap ids)
        {
            refConnections = connections;
            refIds = ids;

            timeoutTimer = new Timer(4000);
            timeoutTimer.AutoReset = true;

            timeoutTimer.Elapsed += timeoutEvent;
            timeoutTimer.Start();
        }

        public void Release()
        {
            timeoutTimer.Close();
            timeoutTimer.Dispose();

            Connection c = this;

            refIds.releaseId(c.id);
            refConnections.TryRemove(id, out c);
        }

        public void ChangeNickname(string nick)
        {
            this.nick = nick;
        }

        private void timeoutEvent(object sender, ElapsedEventArgs e)
        {
            if (timeoutFlag)
            {
                timeoutFlag = false;
                socket.Send(new byte[1] { 0 });
            }
            else
            {
                Console.WriteLine("[Server] {0} reached timeout! Disconnecting...", id);
                Release();
            }
        }
    }
}
