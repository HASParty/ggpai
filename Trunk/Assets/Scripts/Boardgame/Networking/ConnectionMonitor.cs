using UnityEngine;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;


namespace Boardgame.Networking
{
    public class ConnectionMonitor : Singleton<ConnectionMonitor>
    {
        public string GameConnectionStatus { get { return Connection.GameConnectionStatus.ToString(); } }
        public string FeedConnectionStatus { get { return Connection.FeedConnectionStatus.ToString(); } }
        public string Host { get { return Connection.Host; } }
        public int FeedPort { get { return Connection.FeedPort; } }
        public int GamePort { get { return Connection.GamePort; } }

        public bool IsConnected()
        {
            return Connection.GameConnectionStatus == Connection.Status.ESTABLISHED && Connection.FeedConnectionStatus == Connection.Status.ESTABLISHED;
        }

        void Start()
        {
            if (Connect())
            {
                StartCoroutine(Read());
            }
        }

        public void UpdateSettings(string host, int gport, int fport)
        {
            Connection.Host = host;
            Connection.GamePort = gport;
            Connection.FeedPort = fport;
        }

        public bool Connect()
        {
            return Connection.Connect();
        }

        public void Disconnect()
        {
            StopCoroutine(Read());
            Connection.Disconnect();
        }

        IEnumerator Read()
        {
            while (true)
            {
                Connection.Read();
                yield return new WaitForSeconds(0.2f);
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
