using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Boardgame.Configuration;

namespace Boardgame.Networking {
    public class GGPSettings {
        public GGPSettings(float e, float r, float g, float cd, float td, int l, float fa, float sa, float fd, float sd, int cdp, float h, float rnd) {
            Epsilon = e;
            Rave = r;
            Grave = g;
            ChargeDiscount = cd;
            TreeDiscount = td;
            Limit = l;
            FirstAggression = fa;
            SecondAggression = sa;
            FirstDefensiveness = fd;
            SecondDefensiveness = sd;
            ChargeDepth = cdp;
            Horizon = h;
            RandomError = rnd;
        }

        public float Epsilon;
        public float Rave;
        public float Grave;
        public float ChargeDiscount;
        public float TreeDiscount;
        public int   Limit;
        public float FirstAggression;
        public float SecondAggression;
        public float FirstDefensiveness;
        public float SecondDefensiveness;
        public int ChargeDepth;
        public float Horizon;
        public float RandomError;
    }

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
            if (data.IsDone || waitingToEnd) return;
            //Debug.Log(Config.Turns + " " + turnCount);
            if (Config.Turns != -1 && turnCount >= Config.Turns)
            {
                EndGame();
            }
            else if (data.State == GDL.Terminal.FALSE) {
                if (data.IsStart) {
                    if (!data.IsHumanPlayerTurn) {
                        other = data.Control == Player.First ? Player.Second : Player.First;
                    }
                    Connection.Write("ready\n", Connection.feedConnection.GetStream());
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
        bool waitingToEnd = false;
        public void EndGame() {
            StopAllCoroutines();
            Debug.Log("REQUESTING ABORTION");
            endAcked = false;
            waitingToEnd = true;
            Connection.Write("abort\n", Connection.feedConnection.GetStream());            
            StartCoroutine(AbortGame());
        }

        IEnumerator AbortGame()
        {
            while(!endAcked) yield return new WaitForEndOfFrame();
            Write("ABORT " + Config.MatchID);
            waitingToEnd = false;
            IsOver = true;
            Debug.Log("It's over.");
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

        public void WriteFeed(GGPSettings g) {
            string send = string.Format("( update {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} )\n",
                g.Epsilon, g.Rave, g.Grave, g.ChargeDiscount, g.TreeDiscount, g.Limit, g.FirstAggression, g.SecondAggression, g.FirstDefensiveness,
                g.SecondDefensiveness, g.ChargeDepth, g.Horizon, g.RandomError, 40, 40, 40);
            //Debug.Log(send);
            Connection.Write(send, Connection.feedConnection.GetStream());
        }

        // Update is called once per frame
        void Update() {
            if (IsConnected() && !IsOver) {
                if (Connection.feedConnection.Available > 0 || Connection.gameConnection.Available > 0) ReadOnce();
            }
        }
    }
}
