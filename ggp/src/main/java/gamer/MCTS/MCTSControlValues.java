package gamer.MCTS;
import java.util.ArrayList;
import java.util.List;

public class MCTSControlValues{
    public long rave;
    public long grave;
    public long limit;
    public double treeDiscount;
    public double chargeDiscount;
    public double epsilon;
    public long chargeDepth;
    public long horizon;
    public ArrayList<Double> defensiveness;
    public ArrayList<Double> aggression;
    public boolean changed;

    public MCTSControlValues(){
        rave = 100;
        grave = 20;
        epsilon = 0.9f;
        treeDiscount = 0.998f;
        chargeDiscount = 0.99f;
        limit = 0;
        defensiveness = new ArrayList<>();
        defensiveness.add(0.5);
        defensiveness.add(0.5);
        aggression = new ArrayList<>();
        aggression.add(0.1);
        aggression.add(0.1);
        horizon = Integer.MAX_VALUE;
        chargeDepth = Integer.MAX_VALUE;
    }

    public void setRave(double rave){
        this.rave = Math.round(rave);
        changed = true;
    }

    public void setGrave(double grave){
        this.grave = Math.round(grave);
        changed = true;
    }

    public void setEpsilon(double epsilon){
        this.epsilon = epsilon;
    }

    public void setTreeDiscount(double treeDiscount){
        this.treeDiscount = treeDiscount;
    }

    public void setChargeDiscount(double chargeDiscount){
        this.chargeDiscount = chargeDiscount;
    }

    public void setLimit(double limit){
        this.limit = Math.round(limit);
    }

    public void setAggression(List<Double> aggression){
        this.aggression = new ArrayList<>(aggression);
    }

    public void setDefensiveness(List<Double> defensiveness){
        this.defensiveness = new ArrayList<>(defensiveness);
    }

    public void setAll(ArrayList<Double> values){
        setEpsilon(values.get(0));
        setRave(values.get(1));
        setGrave(values.get(2));
        setChargeDiscount(values.get(3));
        setTreeDiscount(values.get(4));
        setLimit(values.get(5));
        if (values.size() > 6){
            setAggression(values.subList(6,8));
            setDefensiveness(values.subList(8,10));
            chargeDepth = Math.round(values.get(10));
            horizon = Math.round(values.get(11));
        }
    }
}
