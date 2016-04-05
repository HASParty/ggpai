package gamer.MCTS;

import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.HashMap;
import java.util.Map;

import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;

// Note: The weird comments are for forcing sane folding with marks.


/**
 * A basic Monte Carlo move wrapper with UCT
 */
@SuppressWarnings("serial")
public class RaveMCMove extends MCMove {
    private List<HashMap<Move, double[]>> rave;
    public static int k = 0;
    private double beta;
    /**
     * A tree node carrying the calculated value of the contained state.
     * @param move The move itself.
     */
    public RaveMCMove(List<Move> move){
        super(move);
    }

    @Override
    public void expand(List<List<Move>> moves){
        leaf = false;
        rave = new ArrayList<HashMap<Move, double[]>>();
        for (int i = 0; i < moves.get(0).size(); i++){
            rave.add(new HashMap<>());
        }
        for (List<Move> move : moves){
            children.put(move, new RaveMCMove(move));
            size++;
            for (int i = 0; i < move.size(); i++){
                rave.get(i).put(move.get(i), new double[]{0.0d, 0.0d});
            }
        }
        
    }

    @Override
    public double calcValue(int win, Map.Entry<List<Move>, MCMove> entry){
        MCMove node = entry.getValue();
        if(node.n == 0){
            return Integer.MAX_VALUE;
        }
        double Qvalue = (node.wins[win] / node.n) + (C * Math.sqrt(Math.log(n)/node.n));
        if(k == 0){
            return Qvalue;
        }
        double raveValue = rave.get(win).get(entry.getKey().get(win))[0];
        return beta * raveValue + (1-beta)*Qvalue;
    }

    @Override
    public List<Move> select(){
        beta = Math.sqrt(k/(3*n+k));
        return super.select();
    }

    public void updateRave(List<List<Move>> jointMoves, List<Double> result){
        for (List<Move> jointMove: jointMoves){
            for (int i = 0; i < jointMove.size(); i++){
                Move m = jointMove.get(i);
                if(rave.get(i).containsKey(m)){
                    double[] val = rave.get(i).get(m);
                    val[0] = ((val[0] * val[1]) + result.get(i))/(val[1]+1);
                    val[1]++;
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

    @Override
    public String toString(){
        String result = "(";
        if(n > 0){
            result += String.format("| n:%8d |",  n);
            result += String.format("value:(%5.1f , %5.1f) | "  , Qvalue(0, this), Qvalue(1, this));
            result += String.format("wins:[%10s , %10s])", f.format(wins[0]), f.format(wins[1]));
        } else {
            result += "<LEAF> ";
        }
        return  result;
    }

}
