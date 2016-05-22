namespace Boardgame.GDL {
    public class ConsideredMove {
        public Move First = null;
        public Move Second = null;
        public float FirstUCT = 0;
        public float SecondUCT = 0;
        public int Simulations = 0;

        public override string ToString() {
            return string.Format("( FIRST: {0} [{1}], SECOND: {2} [{3}], SIMULATIONS: {4} )",
                First, FirstUCT, Second, SecondUCT, Simulations);
        }
    }
}
