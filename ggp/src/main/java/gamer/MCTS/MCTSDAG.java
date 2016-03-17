package gamer.MCTS;

import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.HashMap;
import java.util.Map;
import java.util.HashSet;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import org.ggp.base.player.gamer.statemachine.StateMachineGamer;
import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;
import org.ggp.base.util.statemachine.StateMachine;
import org.ggp.base.util.statemachine.exceptions.GoalDefinitionException;
import org.ggp.base.util.statemachine.exceptions.MoveDefinitionException;
import org.ggp.base.util.statemachine.exceptions.TransitionDefinitionException;



/*
 * A simple MCTS Thread that defines a Monte carlo search algorithm for
 * a tree made up of MCMove nodes.
 */ 
public final class MCTSDAG extends Thread {
    private int counter = 0;
    private int heapCheck = 0;
    private static HashMap<MachineState, MCMove> dag;
    private static final int limit = 0;
    private static final int threads = 1;
    private static Runtime runtime = Runtime.getRuntime();
    private ExecutorService executor;
    private ReentrantReadWriteLock lock;
    private static boolean alive;
    private static boolean expanding;
    protected StateMachineGamer gamer;
    protected StateMachine machine;
    protected MCMove root;
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
    public MCTSDAG(StateMachineGamer gamer, ReentrantReadWriteLock lock, boolean silent){
        this.silent = silent;
        this.gamer = gamer;

        dag = new HashMap<>(100000);
        expanding = true;
        machine = gamer.getStateMachine();
        root = new MCMove(null);
        newRoot = null;
        alive = true;
        this.lock = lock;
        executor  = Executors.newFixedThreadPool(threads);
    }

    @Override
    public void run(){
        //While we are alive we keep on searching
        System.out.println("Using MCTSDAG");
        while(alive){
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
                    if (debug){
                        // printTree(); //Print our new tree
                    }
                }
                if(heapCheck % 1000 == 0){
                    checkHeap();
                }
                heapCheck++;
                search(root, gamer.getCurrentState());
                lock.writeLock().unlock();
            } catch (Exception e){
                System.out.println("EXCEPTION: " + e.toString());
                e.printStackTrace();
                alive = false;
            }
        }
        MCMove.reset(); //Reset N
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
    private List<Integer> search(MCMove node, MachineState state) throws MoveDefinitionException, TransitionDefinitionException, GoalDefinitionException{
        List<Integer> result;
        if(!node.equals(root)){ //If we aren't at the root we change states
            state = node.state;
        }
        if(node.terminal || machine.isTerminal(state)){
            if(node.goals == null){
                node.goals = machine.getGoals(state); //Decreasing terminal calls
                node.terminal = true;
            }
            MCMove.N++;
            result = node.goals;
        } else if (node.leaf()){
            if(expanding){
                node.expand(machine.getLegalJointMoves(state));
            }
            result = playOut(state); //We do one playout
        } else {
            List<Move> ci = node.select();
            MCMove child = node.children.get(ci);
            if(child.n() == 0){ //We only ever use the SM once for each state
                child.state = machine.getNextState(state, ci); 
                if(dag.containsKey(child.state)){
                    counter++;
                    node.children.replace(ci, dag.get(child.state));
                    child = node.children.get(ci);
                } else {
                    dag.put(child.state, child);
                }
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
    private List<Integer> playOut(MachineState state) throws GoalDefinitionException, MoveDefinitionException, TransitionDefinitionException {
        state = machine.performDepthCharge(state, new int[1]);
        return machine.getGoals(state);
    }

    /**
     * @return The best move at this point
     */
    public List<Move> selectMove() throws MoveDefinitionException {
        System.out.println("dag used this turn: " + counter);
        counter = 0;
        Map.Entry<List<Move>, MCMove> bestMove = null;
        if (!silent){
            System.out.println("================================Available moves================================");
        }
        for (Map.Entry<List<Move>, MCMove> entry : root.children.entrySet()){
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
            for (Map.Entry<List<Move>, MCMove> entry: root.children.entrySet()){
                if (moves.get(0).equals(entry.getKey().get(0)) &&
                    moves.get(1).equals(entry.getKey().get(1))){
                    root = entry.getValue();
                    MCMove.N = root.n();
                    if(dag.size() > 50000){
                        HashSet<MachineState> marked =  new HashSet<>();
                        System.out.println("Size of dag before sweep: " + dag.size());
                        mark(root, marked);
                        sweep(marked);
                        System.out.println("Size of dag after sweep: " + dag.size());
                    }
                    return;
                }
            }


                
        }
        throw new IllegalStateException("A move was selected that was not one of the root node moves");
    }
    private void sweep(HashSet<MachineState> marked){
        ArrayList<MachineState> remove = new ArrayList<>();
        for(MachineState state : dag.keySet()){
            if(!marked.contains(state)){
                remove.add(state);
            }
        }
        for(MachineState state : remove){
            dag.remove(state);
        }
    }

    private void mark(MCMove node, HashSet<MachineState> marked){
        if(node.leaf()){
            return;
        }
        if(dag.containsKey(node.state)){
            marked.add(node.state);
        }
        for (MCMove child : node.children.values()){
            mark(child, marked);
        }
    }


    // private void printTree(String indent, MCMove node){
    //     System.out.println(indent + node);
    //     for (MCMove move : node.children){
    //         printTree(indent + "    ", move);
    //     }
    // }
    //
    // #<{(|*
    //  * Pretty prints the tree
    //  |)}>#
    // public void printTree(){
    //     printTree("", root);
    // }
    
    public String SSRatio(){
        return root.SSRatio();
    }

    public String baseEval(){
        String result = "";
        synchronized(root){
            DecimalFormat f = new DecimalFormat("#.##f");
            for(Map.Entry<List<Move>, MCMove> entry : root.children.entrySet()){
                result += "("; 
                result += "m:" + entry.getKey();
                result += " n:" + entry.getValue().n();
                result += " v:[" + f.format(root.calcValue(0, entry.getValue())) + " " + 
                          f.format(root.calcValue(1, entry.getValue())) + "]";
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


    /**
     * Breaks the searcher out of his loop
     */
    public void shutdown(){
        alive = false;
    }

}
