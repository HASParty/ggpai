package gamer.MCTS.nodes;

import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;

import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;

// Note: The weird comments are for forcing sane folding with marks.

/**
 * A Monte Carlo Tree node that combines GRAVE and UCT for selection.
 *
 * Note that this class assumes that there are two players and only one of them has
 * a move each turn.
 */
@SuppressWarnings("serial")
public class RaveNode extends Node {
    //----------Variables and Constructor--------------------------------------------------------{{
    // Variables{{
    private List<HashMap<Move, double[]>> rave;
    private static long graveThresh = 20;
    private HashMap<List<Move>, RaveNode> children;
    private static long k = 0;
    //}}
    //public RaveNode(List<Move> move){{
    /**
     * A tree node carrying the calculated value of the contained state.
     * @param move The move itself.
     */
    public RaveNode(List<Move> move){
        super();
        children = new HashMap<List<Move>, RaveNode>(); //TODO: Replace with a saner mapping
        rave = null;
    }//}}
    //}}

    //----------Main MCTS interface functions----------------------------------------------------{{
    //public void expand(List<List<Move>> moves){{
    /**
     * Generates empty child nodes for each given move and adds them to children
     *
     * @param moves List of joint moves
     */
    public void expand(List<List<Move>> moves){
        leaf = false;
        rave = new ArrayList<HashMap<Move, double[]>>();
        for (int i = 0; i < moves.get(0).size(); i++){
            rave.add(new HashMap<>());
        }
        for (List<Move> move : moves){
            children.put(move, new RaveNode(move));
            size++;
            for (int i = 0; i < move.size(); i++){
                rave.get(i).put(move.get(i), new double[]{0.0d, 0.0d});
            }
        }

    }//}}

    //public List<Move> select(List<HashMap<Move, double[]>> grave){{
    /**
     * Selects the child move that has the highest value
     *
     * @param grave The rave values passed down by the ancestors to be used until the threshold is met
     *
     * @return the child node with the highest UCTGRAVE value
     */
    public List<Move> select(List<HashMap<Move, double[]>> grave){
        double best = -1;
        double best2 = -1;
        Map.Entry<List<Move>, RaveNode> bestMove = null;
        Map.Entry<List<Move>, RaveNode> bestMove2 = null;
        for (Map.Entry<List<Move>, RaveNode> entry : children.entrySet()){
            double value = calcValue(0, entry, grave.get(0));
            double value2 = calcValue(1, entry, grave.get(1));
            // This method of selecting the best move only works because this is made
            // only to work with turn taking games without both players moving in the same turn
            // This will NOT work if that assumption is broken
            if (value > best){
                best = value;
                bestMove = entry;
            }
            if (value2 > best2){
                best2 = value2;
                bestMove2 = entry;
            }
            if (best == best2 && best == Integer.MAX_VALUE) break;
        }
        int i = 0;
        //Again this only works because we are assuming there are always only two players
        ArrayList<Move> result = new ArrayList<>();
        result.add(bestMove.getKey().get(0));
        result.add(bestMove2.getKey().get(1));
        return result;
    } //}}

    //public void updateRave(List<List<Move>> jointMoves, List<Double> result){{
    /**
     * Updates the stored Rave values
     *
     * @param jointMoves List of joint moves that were played further down in this run
     * @param result     The resulting score of this run to update with.
     */
    public void updateRave(List<List<Move>> jointMoves, List<Double> result){
        //A hashset to keep track of which rave values have already been updated
        ArrayList<HashSet<Move>> done = new ArrayList<HashSet<Move>>();
        for (int i = 0; i < rave.size(); ++i) done.add(new HashSet<>());

        for (List<Move> jointMove: jointMoves){
            for (int i = 0; i < jointMove.size(); i++){
                Move m = jointMove.get(i);
                HashMap<Move, double[]> playerRave = rave.get(i);
                if(!done.get(i).contains(m) && playerRave.containsKey(m)){
                    double[] val = playerRave.get(m);
                    val[0] = ((val[0] * val[1]) + result.get(i))/(val[1]+1);
                    val[1]++;
                    done.get(i).add(m);
                }
            }
        }
    }//}}
    //}}

    //----------Getters and Setters--------------------------------------------------------------{{

    //public HashMap<List<Move>, RaveNode> getChildren(){{
    /**
     * Getter for children
     *
     * @return A Mapping from joint moves to child nodes
     */
    public HashMap<List<Move>, RaveNode> getChildren(){
        return children;
    }//}}

    //public static void setGrave(long graveThresh){{
    /**
     * Setter for grave
     *
     * @param graveThresh The new grave threshold
     */
    public static void setGrave(long graveThresh){
        RaveNode.graveThresh = graveThresh;
    }//}}

    //public List<HashMap<Move, double[]>> updateGrave(List<HashMap<Move, double[]>> grave){{
    /**
     * Takes in grave values and updates them/adds to them where it should.
     *
     * @param grave the grave values being built up on the way down
     * @return the grave list, essentially allowing this to be used as a init for them.
     */
    public List<HashMap<Move, double[]>> updateGrave(List<HashMap<Move, double[]>> grave){
        boolean fresh = false; // If there is no grave we need to make it
        if (grave == null) fresh = true;
        if (fresh) grave = new ArrayList<HashMap<Move, double[]>>();
        for (int i = 0; i < rave.size(); i++){
            if(fresh) grave.add(new HashMap<>());
            HashMap<Move, double[]> currGrave = grave.get(i);
            for (Map.Entry<Move, double[]> entry : rave.get(i).entrySet()){
                double[] value = entry.getValue();
                //We only add a grave value if its above the the threshold
                if(value[1] >= graveThresh){
                    Move move = entry.getKey();
                    if (currGrave.putIfAbsent(move, value) != null){
                        currGrave.replace(move, value);
                    }
                }
            }
        }
        return grave;
    }//}}
    //}}

    //----------Helpers--------------------------------------------------------------------------{{
    //private double calcValue(int win, Map.Entry<List<Move>, RaveNode> entry, HashMap<Move, double[]> grave){{
    private double calcValue(int win, Map.Entry<List<Move>, RaveNode> entry, HashMap<Move, double[]> grave){
        double beta = Math.sqrt(k/(3*n+k));
        List<Move> move = entry.getKey();
        RaveNode node = entry.getValue();
        double QValue = QValue(win, node);
        if((QValue > (Integer.MAX_VALUE - 1))){
            return Integer.MAX_VALUE;
        }
        double raveValue;
        if(grave.containsKey(move) && rave.get(win).get(move.get(win))[1] < graveThresh){
            raveValue = grave.get(move.get(win))[0];
        } else {
            raveValue = rave.get(win).get(move.get(win))[0];
        }
        return beta * raveValue + (1-beta)*QValue;
    } //}}

    //public double QValue(int win, RaveNode node){{
    /**
     * @param which  Which roles q value do you want.
     * @param node The node to calculate the Q value of.
     * @return The new UCT value of the move
     */
    public double QValue(int which, RaveNode node){
        if(node.n == 0){
            return Integer.MAX_VALUE;
        }
        return (node.wins[which] / node.n) + (C * Math.sqrt(Math.log(n)/node.n));
    } //}}

    //public String raveToString(){{
    public String raveToString(){
        String res = "";
        for (HashMap<Move, double[]> map : rave){
            for (Map.Entry<Move, double[]> entry: map.entrySet()){
                res += '(' + entry.getKey().toString() + ' ' + entry.getValue()[0] + ')';
            }
            res += "\n\n";
        }
        return res;
    }//}}

    public static void setRaveBias(long k){
        RaveNode.k = k;
    }

    //public String toString(){{
    @Override
    public String toString(){
        String result;
        if(n > 0){
            result = super.toString();
            result += String.format("value:(%5.1f , %5.1f) | "  , QValue(0, this), QValue(1, this));
            result += ")";
        } else {
            result =  super.toString();
        }
        return  result;
    }//}}
    //}}
}
