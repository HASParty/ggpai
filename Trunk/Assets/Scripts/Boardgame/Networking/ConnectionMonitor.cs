using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Boardgame.Configuration;

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

        private string gameID;

        public Player other = Player.Second;

        public bool IsOver;

        public bool IsConnected() {
            return Connection.GameConnectionStatus == Connection.Status.ESTABLISHED && Connection.FeedConnectionStatus == Connection.Status.ESTABLISHED;
        }

        void Start() {
            Disconnect();
            if(Connect()) StartGame();
            turnCount = 0;
            IsOver = false;
            OnGameUpdate.RemoveListener(TurnMonitor);
            OnGameUpdate.AddListener(TurnMonitor);
            BoardgameManager.Instance.OnMakeMove.AddListener(MoveMade);
        }

        public void MoveMade(List<GDL.Move> moves, Player player) {
            if (player == other) {
                turnCount++;
                foreach (var m in moves) {
                    Push(BoardgameManager.Instance.writer.WriteMove(m));
                }
            }
        }

		public void ModifyRequestTime (float modifier)
		{
			requestLeft *= modifier;
		}

		private float requestLeft;
        IEnumerator Request() {
			while(requestLeft > 0f) {
            	yield return new WaitForSeconds(0.1f);
				requestLeft -= 0.1f;
			}
			Pull();
        }

        IEnumerator InitialRequest() {
            yield return new WaitForSeconds(Config.StartTime);
            Pull();
        }

        private int turnCount = 0;
        void TurnMonitor(GameData data) {
            if (data.IsDone) return;
            Debug.Log(Config.Turns + " " + turnCount);
            if (Config.Turns != -1 && turnCount >= Config.Turns)
            {
                EndGame();
            }
            else if (data.State == GDL.Terminal.FALSE) {
                if (data.IsStart && !data.IsHumanPlayerTurn) {
                    other = data.Control == Player.First ? Player.Second : Player.First;
                    Connection.Write(Connection.Compose("ready"), Connection.feedConnection.GetStream());
                }
                if (!data.IsHumanPlayerTurn) {
                    if (data.IsStart) {
                        StartCoroutine(InitialRequest());
                    } else {
						requestLeft = Config.TurnTime;
                        StartCoroutine(Request());
                    }                    
                }
            } else {
                EndGame();
            }
        }

        bool endAcked;
        public void EndGame() {
            StopCoroutine(Request());
            Debug.Log("REQUESTING ABORTION");
            Connection.Write("abort\n", Connection.feedConnection.GetStream());
            endAcked = false;
            StartCoroutine(AbortGame());
        }

        IEnumerator AbortGame()
        {
            while(!endAcked) yield return new WaitForEndOfFrame();
            Write("ABORT " + Config.MatchID);
            IsOver = true;
        }

        void OnDestroy() {
            //EndGame();
        }

        public void Pull() {
            Write("PULL " + Config.MatchID);
        }

        public void Push(string msg) {
            Write("PUSH " + Config.MatchID + " " + msg);
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
            Connection.StartGame(Config.GameName, Config.MatchID, false, Config.StartTime, Config.TurnTime);
        }

        public void Write(string write) {
            Connection.Write(Connection.Compose(write), Connection.gameConnection.GetStream());
        } 

        public void ReadOnce() {
            string data;
            if(Connection.ReadLine(Connection.feedConnection, out data)) {
                //Debug.Log(data);
                if (!data.Contains("ack"))
                {    
                    OnFeedUpdate.Invoke(new FeedData(data));
                    Connection.Write("ack\n", Connection.feedConnection.GetStream());
                }
                else
                {
                    endAcked = true;
                }
            }
            if(Connection.gameConnection.Available > 0) {
                data = Connection.HttpRead(Connection.gameConnection);
                OnGameUpdate.Invoke(new GameData(data));
            }
        }

        // Update is called once per frame
        void Update() {
            if (IsConnected() && !IsOver) {
                if (Connection.feedConnection.Available > 0 || Connection.gameConnection.Available > 0) ReadOnce();
            }
        }
    }
}
