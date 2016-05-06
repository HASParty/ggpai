package gamer.MCTS;

import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.Deque;
import java.util.LinkedList;
import java.util.Arrays;
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

import org.ggp.base.player.gamer.statemachine.StateMachineGamer;
import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;
import org.ggp.base.util.statemachine.StateMachine;
import org.ggp.base.util.statemachine.exceptions.GoalDefinitionException;
import org.ggp.base.util.statemachine.exceptions.MoveDefinitionException;
import org.ggp.base.util.statemachine.exceptions.TransitionDefinitionException;

import gamer.MCTS.MovePick.MAST;
import gamer.MCTS.MovePick.MovePick;
import gamer.MCTS.nodes.UCTNode;


// Note: The weird comments are for forcing sane folding with marks.

/**
 * A simple MCTS Thread that defines a Monte carlo search algorithm for
 * a DAG made up of UCTNode nodes.
 *
 * Mainly used as a baseline tester against MCTSRAVE.
 */
public final class MCTSDAG extends SearchRunner {
    //Variables {{
    //MCTS Enhancement variables {{
    //MCTS DAG
    private HashMap<MachineState, UCTNode> dag;
    //MAST
    private MovePick mast;
    private double epsilon;
    //}}
    //General MCTS variables{{
    private Random rand = new Random();
    private static boolean alive;
    protected UCTNode root;
    //}}
    //Extra info/control variables {{
    private int DagCounter = 0;
    private static final int limit = 0;
    private int lastPlayOutDepth;
    private int playOutCount;
    private float avgPlayOutDepth;
    private double treeDiscount;
    private double chargeDiscount;
    //}}
    //}}
    //MCTSDAG(StateMachineGamer, ReadWriteLock, boolean) {{
    //DocString MCTSDAG {{
    /**
     * Simple constructor
     *
     * @param gamer The gamer using this search
     * @param lock A lock just to be safe
     * @param silent Set to false to make it silent
     * @param epsilon What percentage of cases the search should use MAST
     */ //}}
    public MCTSDAG(StateMachineGamer gamer, ReentrantReadWriteLock lock, //{{
            boolean silent, double epsilon) {
        super(gamer, lock, silent);
        this.epsilon = epsilon;
        lastPlayOutDepth = 0;
        playOutCount = 0;
        avgPlayOutDepth = 0;
        treeDiscount = 0.998f;
        chargeDiscount = 0.99f;

        dag = new HashMap<>(40000);
        mast = new MAST(gameName);
        root = new UCTNode(null);
    } //}}
    //}}
    //MCTS selection phase {{
    //protected  void search() throws MoveDefinitionException,{{
    @Override
    protected  void search() throws MoveDefinitionException,
                                    TransitionDefinitionException,
                                    GoalDefinitionException {
        search(root, gamer.getCurrentState());
    }//}}

    //search(UCTNode, MachineState){{
    /**
     * A recursive MCTS search function that searches through MCM nodes.
     * It only expands one node each run when it hits a new leaf.
     *
     * @param node The node we are searching
     * @param state The current state entering this node
     *
     * @return The simulated value of this node for each player from one simulation.
     * @throws MoveDefinitionException Thrown in the GGP base
     * @throws TransitionDefinitionException Thrown in the GGP base
     * @throws GoalDefinitionException Thrown in the GGP base
     */
    private List<Double> search(UCTNode node,MachineState state) throws MoveDefinitionException,
                                                                        TransitionDefinitionException,
                                                                        GoalDefinitionException{
        List<Double> result;
        if(!node.equals(root)){ //If we aren't at the root we change states
            state = node.state;
        }
        if(node.terminal || machine.isTerminal(state)){
            if(node.goals == null){
                node.goals = getGoalsAsDouble(state); //Decreasing terminal calls
                node.terminal = true;
            }
            result = new ArrayList<>(node.goals);
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
        applyDiscount(result, treeDiscount);
        return result;
    }
    //}}
    //}}
    //MCTS playout phase {{

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
        } else if(lastPlayOutDepth >= 210){
            avgPlayOutDepth = ((avgPlayOutDepth * playOutCount) + lastPlayOutDepth) / (float)(playOutCount + 1);
            playOutCount++;
            return new ArrayList<Double>(Arrays.asList(40.0d, 40.0d));
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
        applyDiscount(result, chargeDiscount);
        return result;
    }
    //}}
    //Move managment{{

    /**
     * @return The best move at this point
     * @throws MoveDefinitionException Thrown in the GGP base
     */
    @Override
    public List<Move> bestMove() throws MoveDefinitionException {
        Map.Entry<List<Move>, UCTNode> bestMove = null;
        for (Map.Entry<List<Move>, UCTNode> entry : root.children.entrySet()){
            if (!silent){
            }
            if (bestMove == null || entry.getValue().n() > bestMove.getValue().n()){
                bestMove = entry;
            }
        }
        System.out.println("------------------");
        System.out.println("Selecting: " + bestMove + " With " + bestMove.getValue().n() + " simulations");
        System.out.println("------------------");
        return bestMove.getKey();
    }

    /**
     * Updates the root node to the given moves
     *
     * @param moves The moves that need to be made
     */
    @Override
    public void applyMove(List<Move> moves)  {
        if (!silent){
            System.out.println("The applied move !: " + moves.toString());
            System.out.println("Average playout depth: " + avgPlayOutDepth);
        }
        synchronized(root){
            for (Map.Entry<List<Move>, UCTNode> entry: root.children.entrySet()){
                if (moves.get(0).equals(entry.getKey().get(0)) &&
                        moves.get(1).equals(entry.getKey().get(1))){

                    markAndSweep(Integer.MAX_VALUE);
                    root = entry.getValue();
                    newRoot = null;
                    playOutCount = 0;
                    avgPlayOutDepth = 0;
                    return;
                }
            }
        }
        throw new IllegalStateException("A move was selected that was not one of the root node moves");
    }

    //}}
    //Helpers up for dag{{
    private void markAndSweep(int depth){
        HashSet<MachineState> marked =  new HashSet<>();
        System.out.println("Size of dag before sweep: " + dag.size());
        long time = System.currentTimeMillis();
        mark(root, marked, depth, time);
        sweep(marked);
        System.out.println("Size of dag after sweep: " + dag.size());
    }

    private void sweep(HashSet<MachineState> marked){
        Iterator<Map.Entry<MachineState, UCTNode>> it = dag.entrySet().iterator();
        long time = System.currentTimeMillis();
        while(it.hasNext()){
            if(!marked.contains(it.next().getKey())){
                it.remove();
                if((System.currentTimeMillis() - time) > 100){
                    System.out.println("Breaking out after " + (System.currentTimeMillis() - time));
                    break;
                }
            }
        }
    }

    private void mark(UCTNode node, HashSet<MachineState> marked, int depth, long time){
        if(node.leaf() || depth == 0){
            return;
        }
        Deque<UCTNode> queue = new LinkedList<>();
        queue.add(node);
        while(queue.size() > 0){
            if((System.currentTimeMillis() - time) > 800){
                break;
            }
            UCTNode child = queue.poll();
            marked.add(child.state);
            for (UCTNode n : child.children.values()){
                queue.add(n);
            }
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

    //}}
    //Helper functions {{
    @Override
    protected  void limitWait() throws InterruptedException{
        if(limit > 0){
            while(root.n() > limit && newRoot == null){
                Thread.sleep(5);
            }
        }
    }
    @Override
    protected  void printStats(){
        System.out.println(String.format("Mast tables size: [%d, %d]",  mast.size(0),
                                                                            mast.size(1)));
        System.out.println("Dag connections made this turn: " + DagCounter);
        DagCounter = 0;

    }
    @Override
    protected  void printMoves(){
        System.out.println("================================Available moves================================");
        System.out.println("N: " + root.n());
        for (Map.Entry<List<Move>, UCTNode> entry : root.children.entrySet()){
            System.out.println("Move: " + entry.getKey() + " " + entry.getValue());
        }
        System.out.println("===============================================================================");

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


    /**
     * Breaks the searcher out of his loop
     */
    public void shutdown(){
        dag = new HashMap<>(100000);
        mast = new MAST(this.gamer.getMatch().getGame().getName());
    }
    //}}
}

// vim: set foldmethod=marker:
// vim: set foldmarker={{,}}:
