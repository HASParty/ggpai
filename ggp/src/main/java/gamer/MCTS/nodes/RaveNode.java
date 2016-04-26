package gamer.MCTS.nodes;

import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;

import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;

// Note: The weird comments are for forcing sane folding with marks.


/**
 * A basic Monte Carlo move wrapper with UCT
 */
@SuppressWarnings("serial")
public class RaveNode extends Node {
    //Variables {{
    private List<HashMap<Move, double[]>> rave;
    private static final int graveThresh = 20;
    private HashMap<List<Move>, RaveNode> children;
    public static int k = 0;
    //}}
    /**
     * A tree node carrying the calculated value of the contained state.
     * @param move The move itself.
     */
    public RaveNode(List<Move> move){
        super();
        children = new HashMap<List<Move>, RaveNode>(); //TODO: Replace with a saner mapping
        rave = null;
    }

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
        
    }
    
    public List<HashMap<Move, double[]>> getMap(){
        return rave;
    }

    public List<HashMap<Move, double[]>> updateGrave(List<HashMap<Move, double[]>> grave){
        if (grave == null){
            grave = new ArrayList<HashMap<Move, double[]>>();
            for(int i = 0; i < rave.size(); i++){
                grave.add(new HashMap<>());
                for (Map.Entry<Move, double[]> entry : rave.get(i).entrySet()){
                    double[] value = entry.getValue();
                    if(value[1] < graveThresh){
                        continue;
                    }
                    Move move = entry.getKey();
                    grave.get(i).put(move, value);
                }
            }
        } else {
            for(int i = 0; i < rave.size(); i++){
                for (Map.Entry<Move, double[]> entry : rave.get(i).entrySet()){
                    double[] value = entry.getValue();
                    if(value[1] < graveThresh){
                        continue;
                    }
                    Move move = entry.getKey();
                    double[] curr = grave.get(i).putIfAbsent(move, value);
                    if (curr != null){
                        grave.get(i).replace(move, value);
                    }
                }
            }
        }
        return grave;
    }

    /**
     * @return The new UCT value of the move
     */
    public double QValue(int win, RaveNode node){
        if(node.n == 0){
            return Integer.MAX_VALUE;
        }
        return (node.wins[win] / node.n) + (C * Math.sqrt(Math.log(n)/node.n));
    }

    public double calcValue(int win, Map.Entry<List<Move>, RaveNode> entry, HashMap<Move, double[]> grave){
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
    }

    /**
     * Sorts the moves and returns the first one.
     *
     * Keep moving, nothing to see here
     *
     * @return the child node with the highest UCT value
     */
    public List<Move> select(List<HashMap<Move, double[]>> grave){
        double best = -1;
        double best2 = -1;
        Map.Entry<List<Move>, RaveNode> bestMove = null;
        Map.Entry<List<Move>, RaveNode> bestMove2 = null;
        for (Map.Entry<List<Move>, RaveNode> entry : children.entrySet()){
            double value = calcValue(0, entry, grave.get(0));
            double value2 = calcValue(1, entry, grave.get(1));
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
        for (List<Move> move : children.keySet()){
            if(bestMove.getKey().get(0).equals(move.get(0)) && bestMove2.getKey().get(1).equals(move.get(1))){
                return move;
            }
        }
        return null; 
    }

    public void updateRave(List<List<Move>> jointMoves, List<Double> result){
        ArrayList<HashSet<Move>> done = new ArrayList<HashSet<Move>>();
        for (int i = 0; i < rave.size(); ++i) done.add(new HashSet<>());
        for (List<Move> jointMove: jointMoves){
            for (int i = 0; i < jointMove.size(); i++){
                Move m = jointMove.get(i);
                if(!done.get(i).contains(m) && rave.get(i).containsKey(m)){
                    double[] val = rave.get(i).get(m);
                    val[0] = ((val[0] * val[1]) + result.get(i))/(val[1]+1);
                    val[1]++;
                    done.get(i).add(m);
                }
            }
        }
    }

    public String raveToString(){
        String res = "";
        for (HashMap<Move, double[]> map : rave){
            for (Map.Entry<Move, double[]> entry: map.entrySet()){
                res += '(' + entry.getKey().toString() + ' ' + entry.getValue()[0] + ')';
            }
            res += "\n\n";
        }
        return res;
    }

    public HashMap<List<Move>, RaveNode> getChildren(){
        return children;
    }

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
    }
}