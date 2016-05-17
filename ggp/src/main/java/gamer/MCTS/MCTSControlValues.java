package gamer.MCTS;
import java.util.ArrayList;
import java.util.List;

public class MCTSControlValues{
    /**Rave K value to set linear rave weight decrease*/
    public long rave;                      
    /**Threshold at which to stop using grave*/
    public long grave;                     
    /**The simulation limit*/
    public long limit;                     
    /**The odds of randomly picking a move instead of picking the best*/
    public double randErr;                 
    /**The goal discount inside of the tree*/
    public double treeDiscount;            
    /**The goal discount in a depth charge*/
    public double chargeDiscount;          
    /**The odds of using MAST rather than picking a random move*/
    public double epsilon;                 
    /**At which depth should a charge be stopped*/
    public long chargeDepth;               
    /**How deep should our tree be allowed to be*/
    public long horizon;                   
    /**UCT C value to control MCTS exploration*/
    public long explorationFactor;         
    /**Used to modulate goal weight of keeping pieces alive*/
    public ArrayList<Double> defensiveness;
    /**Used to modulate goal weight of killing pieces*/
    public ArrayList<Double> aggression;   
    /**The default values used if a charge is stopped*/
    public ArrayList<Double> chargeDefaults;
    /**A control value used to see if a value has changed*/
    public float niceThreshold;
    public boolean changed;                

    //public MCTSControlValues(){{
    /**
     *  Default constructor that sets semi sane defaults
     */
    public MCTSControlValues(){
        explorationFactor = 40;
        rave = 100;
        grave = 20;
        randErr = 0;
        epsilon = 0.0f;
        treeDiscount = 0.999f;
        chargeDiscount = 0.995f;
        limit = 0;
        defensiveness = new ArrayList<>();
        defensiveness.add(0.10);
        defensiveness.add(0.10);
        aggression = new ArrayList<>();
        aggression.add(0.10);
        aggression.add(0.10);
        chargeDefaults = new ArrayList<>();
        chargeDefaults.add(40.00);
        chargeDefaults.add(40.00);
        horizon = Integer.MAX_VALUE;
        chargeDepth = Integer.MAX_VALUE;
    }//}}

    //public MCTSControlValues(ArrayList<Double> values){{
    /**
     * Constructor that uses a list to set the control values
     * <p>
     *  The sixteen variables for the MCTS are<br>
     *  Epsilon<br>
     *  Rave<br>
     *  Grave<br>
     *  Charge Discount<br>
     *  Tree Discount<br>
     *  Limit<br>
     *  Aggression x2<br>
     *  Defensiveness x2<br>
     *  Charge Depth<br>
     *  Horizon<br>
     *  Random Error<br>
     *  Charge Defaults x2<br>
     *  Exploration Factor<br>
     * </p>
     *
     * @param values A list containing the values to be set.  needs to hold 16 doubles
     */

    public MCTSControlValues(ArrayList<Double> values){
        setAll(values);
    }//}}

    //public void setRave(double rave){{
    public void setRave(double rave){
        this.rave = Math.round(rave);
        changed = true;
    }//}}

    //public void setGrave(double grave){{
    public void setGrave(double grave){
        this.grave = Math.round(grave);
        changed = true;
    }//}}

    //public void setEpsilon(double epsilon){{
    public void setEpsilon(double epsilon){
        this.epsilon = epsilon;
    }//}}

    //public void setTreeDiscount(double treeDiscount){{
    public void setTreeDiscount(double treeDiscount){
        this.treeDiscount = treeDiscount;
    }//}}

    //public void setChargeDiscount(double chargeDiscount){{
    public void setChargeDiscount(double chargeDiscount){
        this.chargeDiscount = chargeDiscount;
    }//}}

    //public void setLimit(double limit){{
    public void setLimit(double limit){
        this.limit = Math.round(limit);
    }//}}

    //public void setAggression(List<Double> aggression){{
    public void setAggression(List<Double> aggression){
        this.aggression = new ArrayList<>(aggression);
    }//}}

    //public void setDefensiveness(List<Double> defensiveness){{
    public void setDefensiveness(List<Double> defensiveness){
        this.defensiveness = new ArrayList<>(defensiveness);
    }//}}

    //public void setChargeDefaults(List<Double> chargeDefaults){{
    public void setChargeDefaults(List<Double> chargeDefaults){
        this.chargeDefaults = new ArrayList<>(chargeDefaults);
    }//}}

    //public void setChargeDepth(double depth){{
    public void setChargeDepth(double depth){
        this.chargeDepth = Math.round(depth);
    }//}}

    //public void setExplorationFactor(double explorationFactor){{
    public void setExplorationFactor(double explorationFactor){
        this.explorationFactor = Math.round(explorationFactor);
        changed = true;
    }//}}

    //public void setNiceThreshold(double explorationFactor){{
    public void setNiceThreshold(double niceThreshold){
        this.niceThreshold = Math.round(niceThreshold);
    }//}}

    //public void setHorizon(double horizon){{
    public void setHorizon(double horizon){
        this.horizon = Math.round(horizon);
    }//}}

    //public void setRandErr(double randErr){{
    public void setRandErr(double randErr){
        this.randErr = randErr;
    }//}}

    //public void setAll(ArrayList<Double> values){{
    public void setAll(ArrayList<Double> values){
        setEpsilon(values.get(0));
        setRave(values.get(1));
        setGrave(values.get(2));
        setChargeDiscount(values.get(3));
        setTreeDiscount(values.get(4));
        setLimit(values.get(5));
        setAggression(values.subList(6,8));
        setDefensiveness(values.subList(8,10));
        setChargeDepth(values.get(10));
        setHorizon(values.get(11));
        setRandErr(values.get(12));
        setChargeDefaults(values.subList(13, 15));
        setExplorationFactor(values.get(15));
        setNiceThreshold(values.get(16));
    }//}}

    //public String toString(){{
    @Override
    public String toString(){
        String result;
        result = "Rave: " + rave + " Grave: " + grave + "randErr: " + randErr + " treeDiscount: " + treeDiscount + "chargeDiscount: " + chargeDiscount + " epsilon: " + epsilon + " chargeDepth: "+ chargeDepth + " horizon: " + horizon + " explorationFactor: " + explorationFactor + " defensiveness: " + defensiveness + " aggression: " + aggression + " nice: " + niceThreshold;
        return result;
    }//}}
    
}
