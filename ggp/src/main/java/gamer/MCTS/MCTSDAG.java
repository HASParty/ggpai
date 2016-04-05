package gamer.MCTS;

import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.HashMap;
import java.util.Map;
import java.util.HashSet;
import java.util.Random;
import java.io.File;
import java.io.ObjectOutputStream;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.ObjectInputStream;

import java.util.concurrent.Callable;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import gamer.MCTS.MovePick.MAST;
import gamer.MCTS.MovePick.MovePick;
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
    //Variables{
    //MCTS Enhancement variables {
    //MCTS DAG
    private HashMap<MachineState, MCMove> dag;
    //MAST
    private MovePick mast;
    private double epsilon;
    //}
    //General MCTS variables{
    private Random rand = new Random();
    private ReentrantReadWriteLock lock;
    private static boolean alive;
    protected MCMove root;
    protected StateMachine machine;
    protected StateMachineGamer gamer;
    public List<Move> newRoot;
    //}
    //Extra info/control variables {
    private int DagCounter = 0;
    private static Runtime runtime = Runtime.getRuntime();
    private static boolean expanding;
    private static final int limit = 0;
    private int lastPlayOutDepth;
    private int playOutCount;
    private float avgPlayOutDepth;
    private double discount;
    private String gameName;

    public boolean silent;
    //}
    //}
    //MCTSDAG(StateMachineGamer, ReadWriteLock, boolean){
    /**
     * Simple constructor
     *
     *
     * @param gamer The gamer using this search
     * @param lock A lock just to be safe
     * @param silent Set to false to make it silent
     */
    public MCTSDAG(StateMachineGamer gamer, ReentrantReadWriteLock lock, boolean silent){
        epsilon = 0.0f;
        this.silent = silent;
        this.gamer = gamer;
        lastPlayOutDepth = 0;
        playOutCount = 0;
        avgPlayOutDepth = 0;
        discount = 0.999f;

        dag = new HashMap<>(20000);
        gameName = gamer.getMatch().getGame().getName();
        if(gameName == null){
            gameName = "Mylla";
        }
        mast = new MAST(gameName);
        expanding = true;
        machine = gamer.getStateMachine();
        root = new MCMove(null);
        newRoot = null;
        alive = true;
        this.lock = lock;
    }
    //}
    //MCTS selection phase {
    @Override
    public void run(){
        int heapCheck = 0;
        mast.loadData();
        // loadDag();
        //While we are alive we keep on searching
        System.out.println("Using MCTSDAG");
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
                    playOutCount = 0;
                    avgPlayOutDepth = 0;
                }
                if(heapCheck % 1000 == 0){
                    checkHeap();
                }
                heapCheck++;
                search(root, gamer.getCurrentState());
                lock.writeLock().unlock();
            } catch (InterruptedException e) {
                System.out.println("this never seems to happen?");
                break;
            } catch (Exception e){
                System.out.println("EXCEPTION: " + e.toString());
                e.printStackTrace();
                Thread.currentThread().interrupt();
                return;
            }
        }
        // mast.saveData();
    }


    //search(MCMove, MachineState){
    /**
     * A recursive MCTS search function that searches through MCM nodes.
     * It only expands one node each run when it hits a new leaf.
     *
     * @param node The node we are searching
     * @param state The current state entering this node
     *
     * @return The simulated value of this node for each player from one simulation.
     */
    private List<Double> search(MCMove node, MachineState state) throws MoveDefinitionException, TransitionDefinitionException, GoalDefinitionException{
        List<Double> result;
        if(!node.equals(root)){ //If we aren't at the root we change states
            state = node.state;
        }
        if(node.terminal || machine.isTerminal(state)){
            if(node.goals == null){

                node.goals = getGoalsAsDouble(state); //Decreasing terminal calls
                node.terminal = true;
            }
            MCMove.N++;
            result = new ArrayList<>(node.goals);
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
                    DagCounter++;
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
            mast.update(ci, result);
        }
        node.update(result);
        applyDiscount(result);
        return result;
    }
    //}
    //}
    //MCTS playout phase {
    private List<Double> playOut(MachineState state) throws GoalDefinitionException, MoveDefinitionException, TransitionDefinitionException {
        lastPlayOutDepth = 0;
        return depthCharge(state);
    }

    private List<Double> depthCharge(MachineState state) throws GoalDefinitionException, MoveDefinitionException, TransitionDefinitionException {
        lastPlayOutDepth++;
        List<Double> result;
        if(machine.isTerminal(state)){
            avgPlayOutDepth = ((avgPlayOutDepth * playOutCount) + lastPlayOutDepth) / (float)(playOutCount + 1);
            playOutCount++;
            return getGoalsAsDouble(state);
        }
        List<List<Move>> moves = machine.getLegalJointMoves(state);
        List<Move> chosen;
        if(rand.nextFloat() >= epsilon){
            chosen = moves.get(rand.nextInt(moves.size()));
        } else {
            chosen = mast.pickMove(moves);
        }
        state = machine.getNextState(state, chosen);
        result = depthCharge(state);
        mast.update(chosen, result);
        applyDiscount(result);
        return result;
    }
    //}
    //Move managment{

    /**
     * @return The best move at this point
     */
    public List<Move> selectMove() throws MoveDefinitionException {
        System.out.println("Dag connections made this turn: " + DagCounter);
        DagCounter = 0;
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
            System.out.println("===============================================================================");
        if (!silent){
            System.out.println("------------------");
            System.out.println("Selecting: " + bestMove + " With " + bestMove.getValue().n() + " simulations");
            System.out.println("------------------");
            System.out.println("Mast table size: " + mast.size());
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
            System.out.println("Average playout depth: " + avgPlayOutDepth);
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

                    markAndSweep(Integer.MAX_VALUE);
                    root = entry.getValue();
                    MCMove.N = root.n();
                    return;
                }
            }
        }
        throw new IllegalStateException("A move was selected that was not one of the root node moves");
    }

    //}
    //Helpers up for dag{
    private void markAndSweep(int depth){
        HashSet<MachineState> marked =  new HashSet<>();
        System.out.println("Size of dag before sweep: " + dag.size());
        mark(root, marked, depth);
        sweep(marked);
        System.out.println("Size of dag after sweep: " + dag.size());
    }
    private void sweep(HashSet<MachineState> marked){
        Iterator<Map.Entry<MachineState, MCMove>> it = dag.entrySet().iterator();
        long time = System.currentTimeMillis();
        while(it.hasNext()){
            if(!marked.contains(it.next().getKey())){
                it.remove();
                if((System.currentTimeMillis() - time) > 100){
                    break;
                }
            }
        }
    }

    private void mark(MCMove node, HashSet<MachineState> marked, int depth){
        if(node.leaf() || depth == 0){
            return;
        }
        marked.add(node.state);
        for (MCMove child : node.children.values()){
            mark(child, marked, depth - 1);
        }
    }

    @SuppressWarnings("unchecked")
    public void loadDag(){
        File file = new File("data/dag/" + gameName);
        if(!file.isFile()){
            return;
        }
        try{
            FileInputStream fis = new FileInputStream(file);
            ObjectInputStream ois = new ObjectInputStream(fis);
            dag = (HashMap) ois.readObject();
            fis.close();
            ois.close();
        } catch (Exception e){
            System.out.println("EXCEPTION: " + e.toString());
            e.printStackTrace();
        }
    }

    public void saveDag(){
        // markAndSweep(Integer.MAX_VALUE);
        File file = new File("data/dag/" + gameName);
        try{
            FileOutputStream fos = new FileOutputStream(file);
            ObjectOutputStream oos = new ObjectOutputStream(fos);
            oos.writeObject(dag);
            fos.close();
            oos.close();
        } catch (Exception e){
            System.out.println("EXCEPTION: " + e.toString());
            e.printStackTrace();
        }
    }

    //}
    //Helper functions {

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
    //
    private void checkHeap(){
        if(((runtime.totalMemory() - runtime.freeMemory())/((float)runtime.maxMemory())) >= 0.85f){
            expanding = false;
        } else {
            expanding = true;
        }
    }

    public String SSRatio(){
        return root.SSRatio();
    }

    private List<Double> getGoalsAsDouble(MachineState state)throws GoalDefinitionException{
            List<Double> result = new ArrayList<>();
            for(Integer inte : machine.getGoals(state)){
                result.add((double)inte);
            }
            return result;
    }

    private void applyDiscount(List<Double> lis){
        for(int i = 0; i < lis.size(); ++i){
            lis.set(i, lis.get(i) * discount);
        }
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
        dag = new HashMap<>(100000);
        mast = new MAST(this.gamer.getMatch().getGame().getName());
    }
    //}
}
