namespace Boardgame.Networking {
    public class GGPSettings {
        public GGPSettings(float epsilon, float rave, float grave, float chargeDiscount,
            float treeDiscount, int limit, float firstAggro, float secondAggro, float firstDefense,
            float secondDefense, int chargeDepth, float horizon, float randomErr, int exploration,
            int agreeableness) {
            Epsilon = epsilon;
            Rave = rave;
            Grave = grave;
            ChargeDiscount = chargeDiscount;
            TreeDiscount = treeDiscount;
            Limit = limit;
            FirstAggression = firstAggro;
            SecondAggression = secondAggro;
            FirstDefensiveness = firstDefense;
            SecondDefensiveness = secondDefense;
            ChargeDepth = chargeDepth;
            Horizon = horizon;
            RandomError = randomErr;
            Exploration = exploration;
            Agreeableness = agreeableness;
        }

        public GGPSettings(GGPSettings g) {
            Epsilon = g.Epsilon;
            Rave = g.Rave;
            Grave = g.Grave;
            ChargeDiscount = g.ChargeDiscount;
            TreeDiscount = g.TreeDiscount;
            Limit = g.Limit;
            FirstAggression = g.FirstAggression;
            SecondAggression = g.SecondAggression;
            FirstDefensiveness = g.FirstDefensiveness;
            SecondDefensiveness = g.SecondDefensiveness;
            ChargeDepth = g.ChargeDepth;
            Horizon = g.Horizon;
            RandomError = g.RandomError;
            Exploration = g.Exploration;
            Agreeableness = g.Agreeableness;
        }

        public int Exploration;
        public float Epsilon;
        public float Rave;
        public float Grave;
        public float ChargeDiscount;
        public float TreeDiscount;
        public int Limit;
        public float FirstAggression;
        public float SecondAggression;
        public float FirstDefensiveness;
        public float SecondDefensiveness;
        public int ChargeDepth;
        public float Horizon;
        public float RandomError;
        public int Agreeableness;
    }
}
