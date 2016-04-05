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
public class MCMove implements java.io.Serializable {
    protected static DecimalFormat f = new DecimalFormat("#######.##E0");
    protected final static double C = 40; //Exploration constant
    protected double[] wins;
    protected long n; //how often this node has been selected
    protected int size;
    protected boolean leaf;
    public static long N = 0;
    public HashMap<List<Move>, MCMove> children; //Its children because statemachines are slow
    public List<Double> goals = null;
    public MachineState state; //The move that lead to this state
    public boolean terminal;

    /**
     * A tree node carrying the calculated value of the contained state.
     * @param move The move itself.
     */
    public MCMove(List<Move> move){
        terminal = false;
        wins = new double[] {0, 0};
        n = 0;
        size = 1;
        leaf = true;
        children = new HashMap<List<Move>, MCMove>(); //TODO: Replace with a saner mapping
    }
    // UCBT functions {
    // update(List<Double) {
    /**
     * Takes in the result and updates, wins and simulation counts before calling
     * calcValue() to calculate the new value of the move.
     *
     *
     * @param result  The value we will use to update this move.
     */
    public void update(List<Double> result){
        wins[0] += result.get(0);
        wins[1] += result.get(1);
        n++;
    }
    //} 
    // calcValue(int, MCMove) {
    /**
     * @return The new UCT value of the move
     */
    public double calcValue(int win, Map.Entry<List<Move>, MCMove> entry){
        MCMove node = entry.getValue();
        if(node.n == 0){
            return Integer.MAX_VALUE;
        }
        return (node.wins[win] / node.n) + (C * Math.sqrt(Math.log(n)/node.n));
    }

    /**
     * @return The new UCT value of the move
     */
    public double Qvalue(int win, MCMove node){
        if(node.n == 0){
            return Integer.MAX_VALUE;
        }
        return (node.wins[win] / node.n) + (C * Math.sqrt(Math.log(n)/node.n));
    }
    //}
    // expand(List<List<Move>> {
    /**
     * Expands this node, adding the given legal moves to its children.
     *
     * @param moves A list of the legal moves in this node
     */
    public void expand(List<List<Move>> moves){
        MCMove.N += 1;
        leaf = false;
        for (List<Move> move : moves){
            children.put(move, new MCMove(move));
            size++;
        }
    }
    //}
    // select(){
    /**
     * Sorts the moves and returns the first one.
     *
     * Keep moving, nothing to see here
     *
     * @return the child node with the highest UCT value
     */
    public List<Move> select(){
        double best = -1;
        double best2 = -1;
        Map.Entry<List<Move>, MCMove> bestMove = null;
        Map.Entry<List<Move>, MCMove> bestMove2 = null;
        for (Map.Entry<List<Move>, MCMove> entry : children.entrySet()){
            double value = calcValue(0, entry);
            double value2 = calcValue(1, entry);
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
        //}
    //}
    // Extra evaluation {
    public String SSRatio(){
        return new DecimalFormat("#.##f").format(N / size);
    }
    //}
    // Helpers {
    public boolean leaf(){
        return leaf;
    }
    //}
    // Print helpers{
    /**
     * @return The size of the tree below this node.
     */
    public int size(){
        return size;
    }

    /**
     * Updates this nodes size.
     *
     * @param size The new child size.
     * @param prev The old child size.
     */
    public void size(int size, int prev){
        this.size += (size - prev);
    }

    /**
     * @return the number of simulations this move has
     */
    public long n(){
        return n;
    }
    public double w(int i){
        return wins[i];
    }

    @Override
    public String toString(){
        String result = "(";
        if(n > 0){
            result += String.format("| N: %d | n:%8d | size:%8d | ", N, n, size);
            result += String.format("value:(%5.1f , %5.1f) | "  , Qvalue(0, this), Qvalue(1, this));
            result += String.format("wins:[%10s , %10s])", f.format(wins[0]), f.format(wins[1]));
        } else {
            result += "<LEAF> ";
        }
        return  result;
    }
    //}
}
