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
        public static int FeedPort = 9149;

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

        public static string Compose(string message)
        {
            string formatted = String.Format("( {0} )\r\n", message);
            return String.Format("POST / HTTP/1.0\r\nAccept: text/delim\r\nHost: {0}\r\nSender: UNITY\r\n" +
                                 "Receiver: GAMESERVER\r\nContent-Type: text/acl\r\nContent-Length: {1}\r\n\r\n{2}", Host, formatted.Length, formatted);
        }

        public static void StartGame(string game, bool first, int startTime, int playTime)
        {
            string unityStart = Compose(String.Format("UNITY 1234 {1} {0} {2} {3}", game, (first ? "first" : "second"), startTime, playTime));
            Write(unityStart, gameConnection.GetStream());
            string response = Read(gameConnection);
            Debug.Log(response);
            Write(Compose("ready"), feedConnection.GetStream());
        }

        public static void Write(string message, NetworkStream ns)
        {
            StreamWriter sw = new StreamWriter(ns);

            if (ns.CanWrite)
            {
                try
                {
                    Debug.Log("Sending message: " + message);
                    sw.Write(message);
                    sw.Flush();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    GameConnectionStatus = Status.ERROR;
                }
            }
            else
            {
                Debug.LogError("Can't write to network stream!");
            }

        }
        
        public static string Read(TcpClient connection)
        {
            NetworkStream ns = connection.GetStream();
            StreamReader sr = new StreamReader(ns);

            string response = sr.ReadToEnd();
            Debug.Log(response);
            return response;
        }

        public static string ReadLine(TcpClient connection)
        {
            if (connection.Available > 0)
            {
                NetworkStream ns = connection.GetStream();
                StreamReader sr = new StreamReader(ns);

                string response = sr.ReadLine();
                Debug.Log(response);
                return response;

            }
            else
            {
                Debug.Log("Nothing received from connection.");
                return "";
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
