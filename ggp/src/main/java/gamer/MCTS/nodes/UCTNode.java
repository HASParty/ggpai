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
 * A basic Monte Carlo node with UCT.
 *
 * Note that this class assumes that there are two players and only one of them has
 * a move each turn.
 */
@SuppressWarnings("serial")
public class UCTNode extends Node {
    public HashMap<List<Move>, UCTNode> children; //Its children because statemachines are slow

    //public UCTNode(List<Move> move){{
    /**
     * A tree node carrying the calculated value of the contained state.
     * @param move The move itself.
     */
    public UCTNode(List<Move> move){
        super();
        children = new HashMap<List<Move>, UCTNode>(); //TODO: Replace with a saner mapping
    }//}}
    // UCBT functions {{
    //public void update(List<Double> result){{
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
    }//}}
    //public double QValue(int win, UCTNode node){{
    /**
     * @return The new UCT value of the move
     */
    public double QValue(int win, UCTNode node){
        if(node.n == 0){
            return Integer.MAX_VALUE;
        }
        return (node.wins[win] / node.n) + (C * Math.sqrt(Math.log(n)/node.n));
    } //}}
    //public void expand(List<List<Move>> moves){{
    /**
     * Expands this node, adding the given legal moves to its children.
     *
     * @param moves A list of the legal moves in this node
     */
    public void expand(List<List<Move>> moves){
        leaf = false;
        for (List<Move> move : moves){
            children.put(move, new UCTNode(move));
            size++;
        }
    } //}}
    //public List<Move> select(){{
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
        Map.Entry<List<Move>, UCTNode> bestMove = null;
        Map.Entry<List<Move>, UCTNode> bestMove2 = null;
        for (Map.Entry<List<Move>, UCTNode> entry : children.entrySet()){
            double value = QValue(0, entry.getValue());
            double value2 = QValue(1, entry.getValue());
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
    } //}}
    //}}
    // Print helpers{{
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
