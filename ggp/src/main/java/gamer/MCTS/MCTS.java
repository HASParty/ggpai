package gamer.MCTS;

import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.Random;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import org.ggp.base.player.gamer.statemachine.StateMachineGamer;
import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;
import org.ggp.base.util.statemachine.StateMachine;
import org.ggp.base.util.statemachine.exceptions.GoalDefinitionException;
import org.ggp.base.util.statemachine.exceptions.MoveDefinitionException;
import org.ggp.base.util.statemachine.exceptions.TransitionDefinitionException;

import gamer.MCTS.nodes.UCTNode;



/**
 * A simple MCTS Thread that defines a Monte carlo search algorithm for
 * a tree made up of UCTNode nodes.
 */
public final class MCTS extends Thread {
    private static final int limit = 3000;
    private static final int threads = 1;
    private static Runtime runtime = Runtime.getRuntime();
    private ExecutorService executor;
    private ReentrantReadWriteLock lock;
    private static boolean alive;
    private Random rand = new Random();
    private static boolean expanding;
    protected StateMachineGamer gamer;
    protected StateMachine machine;
    protected UCTNode root;
    private long accum;
    private long count;
    public boolean silent;
    public List<Move> newRoot;
    boolean debug = false;

/**
 * Simple constructor
 *
 *
 * @param gamer The gamer using this search
 * @param lock A lock just to be safe
 * @param silent Set to false to make it silent
 */
    public MCTS(StateMachineGamer gamer, ReentrantReadWriteLock lock, boolean silent){
        this.silent = silent;
        this.gamer = gamer;
        expanding = true;
        machine = gamer.getStateMachine();
        root = new UCTNode(null);
        newRoot = null;
        alive = true;
        this.lock = lock;
    }

    @Override
    public void run(){
        //While we are alive we keep on searching
        System.out.println("Using MCTS");
        while(!Thread.currentThread().isInterrupted()){
            try {
                if(limit > 0){
                    while(root.n() > limit && newRoot == null){
                        Thread.sleep(5);
                    }
                }
                lock.writeLock().lock(); //Making sure the statemachine and tree are in sync
                if (newRoot != null){
                    applyMove(newRoot); //Move to our new root
                    newRoot = null;
                    accum = 0;
                    count = 0;
                }
                checkHeap();
                search(root, gamer.getCurrentState());
                lock.writeLock().unlock();
            } catch (InterruptedException e) {
                lock.writeLock().unlock();
                System.out.println("this never seems to happen?");
                break;
            } catch (Exception e){
                lock.writeLock().unlock();
                System.out.println("EXCEPTION: " + e.toString());
                e.printStackTrace();
                Thread.currentThread().interrupt();
                return;
            }
        }
    }

    private void checkHeap(){
             if(((runtime.totalMemory() - runtime.freeMemory())/((float)runtime.maxMemory())) >= 0.85f){
                 expanding = false;
             } else {
                 expanding = true;
             }
    }

    /**
     * A recursive MCTS search function that searches through MCM nodes.
     * It only expands one node each run when it hits a new leaf.
     *
     * @param node The node we are searching
     * @param state The current state entering this node
     *
     * @return The simulated value of this node for each player from one simulation.
     */
    private List<Double> search(UCTNode node, MachineState state) throws MoveDefinitionException, TransitionDefinitionException, GoalDefinitionException{
        List<Double> result;
        if(!node.equals(root)){ //If we aren't at the root we change states
            state = node.state;
        }
        if(node.terminal || machine.isTerminal(state)){
            if(node.goals == null){
                node.goals = getGoalsAsDouble(state);
                node.terminal = true;
            }
            result = node.goals;
        } else if (node.leaf()){
            if(expanding){
                node.expand(machine.getLegalJointMoves(state));
            }
            result = playOut(state); //We do one playout
        } else {
            List<Move> ci = node.select();
            UCTNode child = node.children.get(ci);
            if(child.n() == 0){ //We only ever use the SM once for each state
                child.state = machine.getNextState(state, ci);
            }
            /* This is ugly but its more efficient than checking them all each time */
            int prev = child.size();
            result = search(child, state);
            node.size(child.size(), prev);
        }
        node.update(result);
        return result;
    }

    /**
     * Performs one depthcharge down to a terminal state
     *
     * @param state The state we start our charge from
     *
     * @return The results of the depth charge for each player
     */
    private List<Double> playOut(MachineState state) throws GoalDefinitionException, MoveDefinitionException, TransitionDefinitionException {
        int[] depth = new int[1];
        List<Double> res = depthCharge(state, depth);
        accum += depth[0];
        count++;
        return res;
    }

    private List<Double> depthCharge(MachineState state, int depth[]) throws GoalDefinitionException, MoveDefinitionException, TransitionDefinitionException {
        List<Double> result;
        if(machine.isTerminal(state)){
            return getGoalsAsDouble(state);
        }
        List<List<Move>> moves = machine.getLegalJointMoves(state);
        List<Move> chosen;
        chosen = moves.get(rand.nextInt(moves.size()));
        state = machine.getNextState(state, chosen);
        depth[0] += 1;
        result = depthCharge(state, depth);
        return result;
    }


    /**
     * @return The best move at this point
     * @throws MoveDefinitionException Thrown in the GGP base
     */
    public List<Move> selectMove() throws MoveDefinitionException {
        Map.Entry<List<Move>, UCTNode> bestMove = null;
        if (!silent){
            System.out.println("================================Available moves================================");
        }
        for (Map.Entry<List<Move>, UCTNode> entry : root.children.entrySet()){
            if (!silent){
                System.out.println("Move: " + entry.getKey() + " " + entry.getValue());
            }
            if (bestMove == null || entry.getValue().n() > bestMove.getValue().n()){
                bestMove = entry;
            }
        }
        if (!silent){
            System.out.println("Selecting: " + bestMove + " With " + bestMove.getValue().n() + " simulations");
        }
        return bestMove.getKey();
    }

    /**
     * Updates the root node to the given moves
     *
     * @param moves The moves that need to be made
     */
    public void applyMove(List<Move> moves)  {
        if (!silent){
            System.out.println("The applied move !: " + moves.toString());
            System.out.println("Average playout depth: " + (accum / (float)count));
        }
        synchronized(root){
            int mb = 1024*1024;

            //Getting the runtime reference from system
            System.out.println("##### Heap utilization statistics [MB] #####");
            //Print used memory
            System.out.println("Used Memory:"
                    + (runtime.totalMemory() - runtime.freeMemory()) / mb);
            //Print free memory
            System.out.println("Free Memory:"
                    + runtime.freeMemory() / mb);
            //Print total available memory
            System.out.println("Total Memory:" + runtime.totalMemory() / mb);
            //Print Maximum available memory
            System.out.println("Max Memory:" + runtime.maxMemory() / mb);
            for (Map.Entry<List<Move>, UCTNode> entry: root.children.entrySet()){
                if (moves.get(0).equals(entry.getKey().get(0)) &&
                    moves.get(1).equals(entry.getKey().get(1))){
                    root = entry.getValue();
                    return;
                }
            }



        }
        throw new IllegalStateException("A move was selected that was not one of the root node moves");
    }

    public String baseEval(){
        String result = "";
        synchronized(root){
            DecimalFormat f = new DecimalFormat("#.##f");
            for(Map.Entry<List<Move>, UCTNode> entry : root.children.entrySet()){
                result += "(";
                result += "m:" + entry.getKey();
                result += " n:" + entry.getValue().n();
                result += " v:[" + f.format(root.QValue(0, entry.getValue())) + " " +
                          f.format(root.QValue(1, entry.getValue())) + "]";
                result += ") ";
            }
        }
        return result;

    }
    /**
     * @return the size of the tree
     */
    public long size(){
        return root.size();
    }

    private ArrayList<Double> getGoalsAsDouble(MachineState state)throws GoalDefinitionException{
            ArrayList<Double> result = new ArrayList<>();
            for(Integer inte : machine.getGoals(state)){
                result.add((double)inte);
            }
            return result;
    }


    /**
     * Breaks the searcher out of his loop
     */
    public void shutdown(){
        alive = false;
    }

}

// vim: set foldmethod=marker:
// vim: set foldmarker={{,}}:
