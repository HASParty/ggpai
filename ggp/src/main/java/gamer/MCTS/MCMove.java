package gamer.MCTS;

import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.List;

import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;


/**
 * A basic Monte Carlo move wrapper with UCT
 */
public class MCMove {
    private static DecimalFormat f;
    private final static double C = 20; //Exploration constant
    final public List<Move> move; //The move that lead to this state
    public static long N = 0;
    private long[] wins;
    private long n; //how often this node has been selected
    private int size;
    public MachineState state; //The move that lead to this state
    public boolean terminal;
    public List<Integer> goals = null;
    public ArrayList<MCMove> children; //Its children because statemachines are slow

    /**
     * This is basically a state node.  I might actually add the state itself to it
     * to sacrifice memory for performance if i think its worth it.
     * @param move The move itself.
     */
    public MCMove(List<Move> move){

        f = new DecimalFormat("#######.##E0");
        terminal = false;
        this.move = move;
        wins = new long[] {0, 0};
        n = 0;
        size = 1;
        children = new ArrayList<MCMove>();
    }

    public float SSRatio(){
        return N / size;
    }


    /**
     * @return the number of wins this move has
     */
    public long w(int who){
        return wins[who];
    }

    /**
     * @return the number of simulations this move has
     */
    public long n(){
        return n;
    }


    /**
     * Takes in the result and updates, wins and simulation counts before calling
     * calcValue() to calculate the new value of the move.
     *
     *
     * @param result  The value we will use to update this move.
     */
    public void update(List<Integer> result, int expanded){
        wins[0] += result.get(0);
        wins[1] += result.get(1);
        if((result.get(0) + result.get(1)) > 100){
            n += 1;
        } else {
            n++;
        }
    }

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
     * @return The new UCT value of the move
     */
    public double calcValue(int win, long N){
        if(n == 0){
            return 1000;
        }
         return (wins[win] / n) + (C * Math.sqrt(Math.log(N)/n));
    }

    /**
     * Expands this node, adding the given legal moves to its children.
     *
     * @param moves A list of the legal moves in this node
     */
    public void expand(List<List<Move>> moves){
        MCMove.N += 1;
        for (List<Move> move : moves){
            children.add(new MCMove(move));
            size++;
        }
    }

    /**
     * Sorts the moves and returns the first one.
     *
     * Keep moving, nothing to see here
     *
     * @return the child node with the highest UCT value
     */
    public MCMove select(){
        double best = 0;
        double best2 = 0;
        MCMove bestMove = null;
        MCMove bestMove2 = null;
        for (MCMove move : children){
            double value = move.calcValue(0, n);
            double value2 = move.calcValue(1, n);
            if (value > best){
                best = value;
                bestMove = move;
            }
            if (value2 > best2){
                best2 = value2;
                bestMove2 = move;
            }
        }
        //Sorry...   I will rewrite this i promise
        for (MCMove move : children){
            if(bestMove.move.get(0).equals(move.move.get(0)) && bestMove2.move.get(1).equals(move.move.get(1))){
                bestMove = move;
            }
        }
        return bestMove;
    }



    /**
     * Resets N to 0;
     */
    public static void reset(){
        N = 0;
    }

    @Override
    public String toString(){
        String result = "(";
        result +=  "Move: " + ((move != null)? move.toString().replace("noop", "n") : null) + " ";
        if(n > 0){
            result += String.format("| N: %d | n:%8d | size:%8d | ", N, n, size);
            result += String.format("value:(%5.1f , %5.1f) | "  , calcValue(0, N), calcValue(1, N));
            result += String.format("wins:[%10s , %10s])", f.format(wins[0]), f.format(wins[1]));
        } else {
            result += "<LEAF> ";
        }
        return  result;
    }
}
