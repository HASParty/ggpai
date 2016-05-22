namespace Boardgame.Networking {
    public struct FMove {
        public GDL.Move Move;
        public int Simulations;
        public Player Who;
        public float FirstUCT;
        public float SecondUCT;
        public override string ToString() {
            return string.Format("{0} s: {1} p: {2} fUCT: {3} sUCT: {4}",
                Move, Simulations, Who, FirstUCT, SecondUCT);
        }
    }
}
