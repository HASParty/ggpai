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

        public Player other = Player.Second;


        public bool IsConnected() {
            return Connection.GameConnectionStatus == Connection.Status.ESTABLISHED && Connection.FeedConnectionStatus == Connection.Status.ESTABLISHED;
        }

        void Start() {
            if(Connect()) StartGame();
            OnGameUpdate.AddListener(TurnMonitor);
            turnTimer = 5f; //later read from config
            gameID = "1234";
            BoardgameManager.Instance.OnMakeMove.AddListener(MoveMade);
        }

        public void MoveMade(List<GDL.Move> moves, Player player) {
            if (player == other) {
                foreach (var m in moves) {
                    Push(BoardgameManager.Instance.writer.WriteMove(m));
                }
            }
        }

        bool requesting = false;
        IEnumerator Request() {
            if (!requesting) {
                requesting = true;
                yield return new WaitForSeconds(turnTimer);
                Pull();
                requesting = false;
            } else {
                Debug.Log("Duplicate pull request");
            }
        }

        void TurnMonitor(GameData data) {
            if (data.State == GDL.Terminal.FALSE) {
                if (data.IsStart && !data.IsHumanPlayerTurn) {
                    other = data.Control == Player.First ? Player.Second : Player.First;
                }
                if (!data.IsHumanPlayerTurn) {
                    StartCoroutine(Request());
                }
            }
        }

        public void EndGame() {
            Write("STOP " + gameID + " ( nil )");
        }

        void OnDestroy() {
            Write("ABORT " + gameID);
            Disconnect();
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
