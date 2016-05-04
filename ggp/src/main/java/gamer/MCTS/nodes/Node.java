package gamer.MCTS.nodes;

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
public abstract class Node implements java.io.Serializable {
    protected static DecimalFormat f = new DecimalFormat("##.#E0");
    protected static double C = 40; //Exploration constant
    protected double[] wins;
    protected long n; //how often this node has been selected
    protected int size;
    protected boolean leaf;
    public static long N = 0;
    public List<Double> goals = null;
    public MachineState state; //The move that lead to this state
    public boolean terminal;

    /**
     * A tree node carrying the calculated value of the contained state.
     * @param move The move itself.
     */
    public Node(){
        terminal = false;
        wins = new double[] {0, 0};
        n = 0;
        size = 1;
        leaf = true;
    }

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

    public void setExplorationFactor(long C){
        this.C = C;
    }

    public double w(int i){
        return wins[i];
    }

    @Override
    public String toString(){
        String result;
        if(n > 0){
            result = "(";
            result += String.format("|n:%8d | ", n);
        } else {
            result = "(<LEAF>)";
        }
        return  result;
    }
    //}
}
