using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Boardgame.Networking
{
    static class Connection
    {
        public enum Status
        {
            OFF,
            ESTABLISHING,
            ESTABLISHED,
            ERROR
        }

        public static string Host = "127.0.0.1";
        public static int GamePort = 9147;
        public static int FeedPort = 9155;

        public static Status FeedConnectionStatus = Status.OFF;
        public static Status GameConnectionStatus = Status.OFF;

        public static TcpClient feedConnection;
        public static TcpClient gameConnection;

        public static bool Connect()
        {
            if (GameConnectionStatus == Status.ERROR || GameConnectionStatus == Status.OFF)
            {
                GameConnectionStatus = Status.ESTABLISHING;

                try
                {
                    gameConnection = new TcpClient(Host, GamePort);
                    GameConnectionStatus = Status.ESTABLISHED;
                }
                catch (Exception e)
                {
                    GameConnectionStatus = Status.ERROR;
                    Debug.LogError(e.ToString());
                    return false;
                }
            }

            if (FeedConnectionStatus == Status.ERROR || FeedConnectionStatus == Status.OFF)
            {
                FeedConnectionStatus = Status.ESTABLISHING;

                try
                {
                    feedConnection = new TcpClient(Host, FeedPort);
                    FeedConnectionStatus = Status.ESTABLISHED;
                }
                catch (Exception e)
                {
                    FeedConnectionStatus = Status.ERROR;
                    Debug.LogError(e.ToString());
                    return false;
                }
            }

            return true;
        }

        public static void Write(string message)
        {
            string formatted = String.Format("( UNITY {0} )\r\n", message);
            string header = String.Format("POST / HTTP/1.0\r\nAccept: text/delim\r\nHost: {0}\r\nSender: UNITY\r\n"+
                                          "Receiver: GAMESERVER\r\nContent-Type: text/acl\r\nContent-Length: {1}\r\n\r\n", Host, formatted.Length);
            string write = header + formatted;
            NetworkStream ns = gameConnection.GetStream();
            StreamWriter sw = new StreamWriter(ns);

            try {
                Debug.Log("Sending message: " + write);
                sw.Write(write);
            } catch(Exception e) {
                Debug.LogError(e.ToString());
                GameConnectionStatus = Status.ERROR;
            }

        }
        
        public static void Read()
        {
            if (gameConnection.Available > 0)
            {
                NetworkStream ns = gameConnection.GetStream();
                StreamReader sr = new StreamReader(ns);

                string response = sr.ReadToEnd();

                Debug.Log("Read from game connection:" + response);
            }
            else
            {
                Debug.Log("Nothing received from game connection.");
            }
            if(feedConnection.Available > 0) {
                NetworkStream ns = feedConnection.GetStream();
                StreamReader sr = new StreamReader(ns);

                string response = sr.ReadToEnd();

                Debug.Log("Read from feed connection:" + response);
            }
            else
            {
                Debug.Log("Nothing received from feed.");
            }
        }

        public static void Disconnect()
        {
            if (gameConnection != null)
            {
                gameConnection.Close();
                GameConnectionStatus = Status.OFF;
            }
            if (feedConnection != null)
            {
                feedConnection.Close();
                FeedConnectionStatus = Status.OFF;
            }
        }
    }
}
