using System;
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
        
        public static void Read()
        {
            NetworkStream ns = gameConnection.GetStream();

            byte[] bytes = new byte[1024];
            int bytesRead = ns.Read(bytes, 0, bytes.Length);

            Debug.Log("Read from game connection:" + Encoding.ASCII.GetString(bytes, 0, bytesRead));

            ns = feedConnection.GetStream();

            bytes = new byte[1024];
            bytesRead = ns.Read(bytes, 0, bytes.Length);

            Debug.Log("Read from feed connection:" + Encoding.ASCII.GetString(bytes, 0, bytesRead));
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
