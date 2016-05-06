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
 * A basic Monte Carlo tree node base.
 *
 * Note that this class assumes that there are two players and only one of them has
 * a move each turn.
 */
@SuppressWarnings("serial")
public abstract class Node implements java.io.Serializable {
    protected static DecimalFormat f = new DecimalFormat("##.#E0");
    protected static double C = 40; //Exploration constant
    protected double[] wins;
    protected long n; //how often this node has been selected
    protected int size;
    protected boolean leaf;
    public ArrayList<Double> goals = null;
    public MachineState state; //The move that lead to this state
    public boolean terminal;

    //public Node(){{
    /**
     * A tree node carrying the calculated value of the contained state.
     */
    public Node(){
        terminal = false;
        wins = new double[] {0, 0};
        n = 0;
        size = 1;
        leaf = true;
    }//}}

    //public void update(List<Double> result){{
    /**
     * Takes in the result and updates, wins and simulation counts before calling
     * calcValue() to calculate the new value of the move.
     *
     * @param result  The value we will use to update this move.
     */
    public void update(List<Double> result){
        wins[0] += result.get(0);
        wins[1] += result.get(1);
        n++;
    }//}}

    // Helpers {{
    //public boolean leaf(){{
    /**
     * @return True if the node is a leaf and hasn't been expanded
     */
    public boolean leaf(){
        return leaf;
    }//}}
    //}}

    // Print helpers{{
    //public int size(){{
    /**
     * @return The size of the tree below this node.
     */
    public int size(){
        return size;
    }//}}

    //public void size(int size, int prev){{
    /**
     * Updates this nodes size.
     *
     * @param size The new child size.
     * @param prev The old child size.
     */
    public void size(int size, int prev){
        this.size += (size - prev);
    } //}}

    //public long n(){{
    /**
     * @return the number of simulations this move has
     */
    public long n(){
        return n;
    }//}}

    //public void setExplorationFactor(long C){{
    /**
     * Allows setting the algorithms explorationFactor
     *
     * @param C the exploration factor to set to
     */
    public static void setExplorationFactor(long C){
        Node.C = C;
    }//}}

    //public double w(int i){{
    /**
     * Returns the accumulated score of player i, Not entirely sure why
     *
     * @param i Which players score do you want
     * @return the ith players score
     */
    public double w(int i){
        return wins[i];
    }//}}

    //public String toString(){{
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
    }//}}
    //}}
}
// vim: set foldmethod=marker:
// vim: set foldmarker={{,}}:
