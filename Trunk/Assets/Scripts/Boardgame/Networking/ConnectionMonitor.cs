using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Boardgame.Networking {
    public class ConnectionMonitor : Singleton<ConnectionMonitor> {
        public string GameConnectionStatus { get { return Connection.GameConnectionStatus.ToString(); } }
        public string FeedConnectionStatus { get { return Connection.FeedConnectionStatus.ToString(); } }
        public string Host { get { return Connection.Host; } }
        public int FeedPort { get { return Connection.FeedPort; } }
        public int GamePort { get { return Connection.GamePort; } }

        [System.Serializable]
        public class FeedEvent : UnityEvent<FeedData> { };
        public FeedEvent OnFeedUpdate;
        [System.Serializable]
        public class GameAIEvent : UnityEvent<GameData> { };
        public GameAIEvent OnGameUpdate;

        private float turnTimer;
        private string gameID;

        Player human;


        public bool IsConnected() {
            return Connection.GameConnectionStatus == Connection.Status.ESTABLISHED && Connection.FeedConnectionStatus == Connection.Status.ESTABLISHED;
        }

        void Start() {
            if(Connect()) StartGame();
            OnGameUpdate.AddListener(FinishInit);
            turnTimer = 5f; //later read from config
            gameID = "1234";
            BoardgameManager.Instance.OnMakeMove.AddListener(MoveMade);
        }

        public void MoveMade(List<GDL.Move> moves, Player player) {
            if (player == human) {
                foreach (var m in moves) {
                    Push(BoardgameManager.Instance.writer.WriteMove(m));
                }
                StartCoroutine(Request());
            }
        }

        IEnumerator Request() {
            yield return new WaitForSeconds(turnTimer);
            Pull();
        }

        void FinishInit(GameData data) {
            if(data.IsStart && data.LegalMoves.Count == 0) {
                human = Player.Black;
                Push("nil");
            } else {
                human = Player.White;
                OnGameUpdate.RemoveListener(FinishInit);
            }
        }

        public void Pull() {
            Write("PULL " + gameID);
        }

        public void Push(string msg) {
            Write("PUSH " + gameID + " " + msg);
        }

        public void UpdateSettings(string host, int gport, int fport) {
            Connection.Host = host;
            Connection.GamePort = gport;
            Connection.FeedPort = fport;
        }

        public bool Connect() {
            return Connection.Connect();
        }

        public void Disconnect() {
            Connection.Disconnect();
        }

        public void StartGame() {
            Connection.StartGame("mylla", false, 5, 5);
        }

        public void Write(string write) {
            Connection.Write(Connection.Compose(write), Connection.gameConnection.GetStream());
        } 

        public void ReadOnce() {
            string data;
            if(Connection.ReadLine(Connection.feedConnection, out data)) {
                OnFeedUpdate.Invoke(new FeedData(data));
            }
            if(Connection.gameConnection.Available > 0) {
                data = Connection.HttpRead(Connection.gameConnection);
                OnGameUpdate.Invoke(new GameData(data));
            }
        }

        // Update is called once per frame
        void Update() {
            if (IsConnected()) {
                if (Connection.feedConnection.Available > 0 || Connection.gameConnection.Available > 0) ReadOnce();
            }
        }
    }
}
