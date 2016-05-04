package gamer.MCTS;
// Imports {{
import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.Deque;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Random;
import com.google.common.cache.Cache;
import com.google.common.cache.CacheBuilder;

import java.util.concurrent.locks.ReentrantReadWriteLock;

import org.ggp.base.player.gamer.statemachine.StateMachineGamer;
import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;
import org.ggp.base.util.statemachine.StateMachine;
import org.ggp.base.util.statemachine.exceptions.GoalDefinitionException;
import org.ggp.base.util.statemachine.exceptions.MoveDefinitionException;
import org.ggp.base.util.statemachine.exceptions.TransitionDefinitionException;
import org.ggp.base.util.gdl.grammar.GdlSentence;

import gamer.MCTS.MovePick.MAST;
import gamer.MCTS.MovePick.MovePick;
import gamer.MCTS.nodes.RaveNode;
import gamer.JeffGamer;
import gamer.UnityGamer;
import util.QueryBuilder;
// }}

// Note: The weird comments are for forcing sane folding with marks.
/*
 * A simple MCTS Thread that defines a Monte carlo search algorithm for
 * a tree made up of RaveNode nodes.
 */
public class MCTSRAVE extends Thread {
    //----------------MCTS Variables----------------------{{
    //MCTS Enhancement variables {{
    //MCTS DAG
    private HashMap<MachineState, RaveNode> dag;
    //MAST
    private MovePick mast;
    private int counter = 0;
    private ArrayList<GdlSentence> pcQuery;
    //Aggression cache
    private Cache<MachineState, ArrayList<Double>> goalCache;
    //}}
    //General MCTS variables{{
    private Random rand = new Random();
    private ReentrantReadWriteLock lock;
    protected RaveNode root;
    protected StateMachine machine;
    protected StateMachineGamer gamer;
    public List<Move> newRoot;
    //}}
    //Extra info/control variables {{
    private MCTSControlValues values;
    private int DagCounter = 0;
    private static Runtime runtime = Runtime.getRuntime();
    private static boolean expanding;
    private int lastPlayOutDepth;
    private int playOutCount;
    private float avgPlayOutDepth;
    private String gameName;
    private ArrayList<Integer> maxCounts;

    public boolean silent;
    //}}
    //}}
    //public MCTSRAVE(StateMachineGamer gamer, ReentrantReadWriteLock lock,{{
    /**
     * Constructor for a MCTS class using GRAVE and MAST
     *
     * @param gamer          The gamer using this search
     * @param lock           A lock just to be safe
     * @param silent         Set to false to make it silent
     * @param epsilon        The epsilon greedy value controlling the use of MAST
     * @param k              The RAVE controll threshold
     * @param grave          The GRAVE threshold
     * @param treeDiscount   How much we discount return values in the MCTS tree
     * @param ChargeDiscount How much we discount return values in the depth charge
     * @param limit          Sets a simulation limit for the AI
     */
    public MCTSRAVE(StateMachineGamer gamer, ReentrantReadWriteLock lock,
                    boolean silent, MCTSControlValues values){
        this.maxCounts = new ArrayList<>();
        maxCounts.add(0);
        maxCounts.add(0);
        this.values = values;
        this.gamer = gamer;
        this.lock = lock;
        this.machine = gamer.getStateMachine();
        this.newRoot = null;
        this.root = new RaveNode(null);
        this.root.state = gamer.getCurrentState();
        expanding = true;

        this.lastPlayOutDepth = 0;
        this.playOutCount = 0;
        this.avgPlayOutDepth = 0;

        RaveNode.k = Math.round(values.rave);
        RaveNode.setGrave(Math.round(values.grave));

        this.gameName = gamer.getMatch().getGame().getName();
        if(gameName == null){
            gameName = "Mylla";
        } else {
            System.out.println(gameName);
        }
        this.dag = new HashMap<>(40000);
        this.mast = new MAST(gameName);
        this.pcQuery = QueryBuilder.pieceCount("pieces_on_board", machine.getRoles());
        this.goalCache = CacheBuilder.newBuilder().maximumSize(40000).build();

        //Debug
        this.silent = silent;
    }
    //}}

    //----------------MCTS selection phase----------------{{
    //public void run(){{
    @Override
    public void run(){
        int heapCheck = 0;
        //While we are alive we keep on searching
        System.out.println("Using MCTSRAVE");
        while(!Thread.currentThread().isInterrupted()){
            try {
                if(values.limit > 0){
                    while(root.n() > values.limit && newRoot == null){
                        Thread.sleep(5); //Not necessary to do this in a more efficient way
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
                search(root, null, new ArrayList<List<Move>>(), 0);
                lock.writeLock().unlock();
            } catch (InterruptedException e) {
                if (lock.writeLock().isHeldByCurrentThread()){
                    lock.writeLock().unlock();
                }
                System.out.println("Interrupted");
                break;
            } catch (Exception e){
                lock.writeLock().unlock();
                System.out.println("EXCEPTION: " + e.toString());
                e.printStackTrace();
                return;
            }
        }
    }//}}

    //private List<Double> search(RaveNode node,{{
    /**
     * A recursive MCTS search function that searches through MCM nodes.
     * It only expands one node each run when it hits a new leaf.
     *
     * @return The simulated value of this node for each player from one simulation.
     */
    private List<Double> search(RaveNode node,
                                List<HashMap<Move, double[]>> grave,
                                List<List<Move>> rave,
                                int depth) throws MoveDefinitionException,
                                                  TransitionDefinitionException,
                                                  GoalDefinitionException{
        List<Double> result;
        MachineState state = node.state;
        if(node.terminal || machine.isTerminal(state)){
            if(node.goals == null){
                node.goals = goalCache.getIfPresent(state);
                if(node.goals == null){
                    node.goals = getGoalsAsDouble(state); //Decreasing terminal calls
                    goalCache.put(state, new ArrayList<>(node.goals));
                }
                node.terminal = true;
            }
            result = new ArrayList<>(node.goals);
        } else if (node.leaf()){
            if(expanding && (depth < values.horizon)){ // We don't expand if we have hit a resource limit
                node.expand(machine.getLegalJointMoves(state));
            }
            result = playOut(state, rave); //We do one playout
        } else {
            grave = node.updateGrave(grave); //We add to our grave values
            List<Move> ci = node.select(grave);
            HashMap<List<Move>, RaveNode> children = node.getChildren();
            RaveNode child =  children.get(ci);
            if(child.n() == 0){ //We only ever use the SM once for each state
                child.state = machine.getNextState(state, ci);
                // Put the node into the dag if its absent
                RaveNode dagNode = dag.putIfAbsent(child.state, child);
                if(dagNode != null){
                    // Otherwise we replace the child with the stored node
                    DagCounter++;
                    children.replace(ci, dagNode);
                    child = children.get(ci);
                }
            }
            grave = node.updateGrave(grave); // We update our grave values on the way down.
            result = search(child, grave, rave, depth + 1);
            rave.add(ci); // We build our rave list on the way up.
            mast.update(ci, result); // We update our mast values on the way up.
        }
        node.update(result);
        if(!node.terminal){
            node.updateRave(rave, result);
        }
        applyDiscount(result, values.treeDiscount);
        return result;
    }//}}
    //}}

    //----------------MCTS playout phase------------------{{
    //private List<Double> playOut(MachineState state,{{
    private List<Double> playOut(MachineState state,
                                 List<List<Move>> rave) throws GoalDefinitionException,
                                                               MoveDefinitionException,
                                                               TransitionDefinitionException {
        lastPlayOutDepth = 0;
        return depthCharge(state, rave);
    }//}}

    //private List<Double> depthCharge(MachineState state,{{
    private List<Double> depthCharge(MachineState state,
                                     List<List<Move>> rave) throws GoalDefinitionException,
                                                                   MoveDefinitionException,
                                                                   TransitionDefinitionException {
        lastPlayOutDepth++;
        List<Double> result;
        if(machine.isTerminal(state)){
            updateAverageDepth();
            ArrayList<Double> goals = goalCache.getIfPresent(state);
            if(goals != null){
                return goals;
            } else {
                counter++;
            }
            goals = getGoalsAsDouble(state);
            applyPersonalityBias(goals, state);
            goalCache.put(state, goals);
            return goals;
        } else if (lastPlayOutDepth > values.chargeDepth){  //If we hit our depth limit
            updateAverageDepth();
            ArrayList<Double> goals = new ArrayList<>();
            goals.add(values.chargeDefaults.get(0));
            goals.add(values.chargeDefaults.get(1)); //We create a default value
            applyPersonalityBias(goals, state); //And apply bias to it
            return goals;
        }

        List<List<Move>> moves = machine.getLegalJointMoves(state);
        List<Move> chosen;
        if(rand.nextFloat() >= values.epsilon){
            chosen = moves.get(rand.nextInt(moves.size()));
        } else {
            chosen = mast.pickMove(moves);
        }
        state = machine.getNextState(state, chosen);
        result = depthCharge(state, rave);
        rave.add(chosen);
        mast.update(chosen, result);
        applyDiscount(result, values.chargeDiscount);
        return result;
    }//}}
    //}}

    //----------------MCTS DAG Helpers--------------------{{
    //private void markAndSweep(int depth){{
    private void markAndSweep(int depth){
        HashSet<MachineState> marked =  new HashSet<>();
        System.out.println("Size of dag before sweep: " + dag.size());
        long time = System.currentTimeMillis();
        mark(root, marked, depth, time);
        sweep(marked);
        System.out.println("Size of dag after sweep: " + dag.size());
   }//}}

    //private void sweep(HashSet<MachineState> marked){{
    private void sweep(HashSet<MachineState> marked){
        Iterator<Map.Entry<MachineState, RaveNode>> it = dag.entrySet().iterator();
        long time = System.currentTimeMillis();
        while(it.hasNext()){
            if(!marked.contains(it.next().getKey())){
                it.remove();
                if((System.currentTimeMillis() - time) > 100){
                    break;
                }
            }
        }
    } //}}

    //private void mark(RaveNode node, HashSet<MachineState> marked, int depth){{
    private void mark(RaveNode node, HashSet<MachineState> marked, int depth, long time){
        if(node.leaf() || depth == 0){
            return;
        }
        Deque<RaveNode> queue = new LinkedList<>();
        queue.add(node);
        while(queue.size() > 0){
            if((System.currentTimeMillis() - time) > 800){
                break;
            }
            RaveNode child = queue.poll();
            marked.add(child.state);
            for (RaveNode n : child.getChildren().values()){
                queue.add(n);
            }
        }
    }//}}
    //}}

    //----------------MCTS Playstyle Modulation-----------{{
    //private ArrayList<Integer> getPieceCount(MachineState state){{
    private ArrayList<Integer> getPieceCount(MachineState state){
        ArrayList<Integer> res = new ArrayList<>();
        for (GdlSentence query : pcQuery){
            GdlSentence answer = ((JeffGamer)gamer).prover.askOne(query, state.getContents());
            res.add(Integer.parseInt(answer.get(0).toSentence().get(1).toString()));
        }
        return res;
    } //}}

    //private void applyPersonalityBias(ArrayList<Double> goals, MachineState state){{
    private void applyPersonalityBias(ArrayList<Double> goals, MachineState state){
        ArrayList<Integer> counts = getPieceCount(state);
        if (counts.size() >= 2){
            maxCounts.set(0, Math.max(maxCounts.get(0), counts.get(0)));
            maxCounts.set(1, Math.max(maxCounts.get(1), counts.get(1)));
            // Defensiveness
            goals.set(0,
                      Math.max(goals.get(0) - ((maxCounts.get(0) - counts.get(0)) * values.defensiveness.get(0)),
                             0.0));
            goals.set(1,
                      Math.max(goals.get(1) - ((maxCounts.get(1) - counts.get(1)) * values.defensiveness.get(1)),
                               0.0));
            // Aggression
            goals.set(0,
                      Math.min(goals.get(0) + ((maxCounts.get(1) - counts.get(1)) * values.aggression.get(0)),
                               100.0));
            goals.set(1,
                      Math.min(goals.get(1) + ((maxCounts.get(0) - counts.get(0)) * values.aggression.get(1)),
                               100.0));
        }
    }//}}
    //}}

    //----------------MCTS Move managment-----------------{{
    //public List<Move> selectMove() throws MoveDefinitionException {{
    /**
     * @return The most simulated move at this point
     */
    public List<Move> selectMove() throws MoveDefinitionException {
        Map.Entry<List<Move>, RaveNode> bestMove = null;
        if (!silent){
            int mb = 1024*1024;
            long total = runtime.totalMemory();
            long free = runtime.freeMemory();

            //Getting the runtime reference from system
            System.out.println("###################### " +
                               "Heap utilization statistics [MB] " +
                               "#######################");
            //Print used memory
            System.out.println(String.format("Used Memory:  %d",
                                            (total - free) / mb));
            //Print free memory
            System.out.println(String.format("Free Memory:  %d", free / mb));
            //Print total available memory
            System.out.println(String.format("Total Memory: %d", total / mb));
            //Print Maximum available memory
            System.out.println(String.format("Max Memory:   %d", runtime.maxMemory() / mb));

            System.out.println();
            System.out.println("================================"+
                               "Available moves"+
                               "================================");
            System.out.println("N: " + root.n());
        }
        boolean whoops = false;
        if(rand.nextFloat() <= values.randErr){
            System.out.println("Whoops, selecting a random move");
            whoops = true;

        }
        for (Map.Entry<List<Move>, RaveNode> entry : root.getChildren().entrySet()){
            if (!silent){
                System.out.println(String.format(" Move: %-40s %-40s",
                            entry.getKey(),
                            entry.getValue()));
            }
            if (whoops){
                if (bestMove == null || entry.getValue().n() < bestMove.getValue().n()){
                    bestMove = entry;
                }

            } else {
                if (bestMove == null || entry.getValue().n() > bestMove.getValue().n()){
                    bestMove = entry;
                }
            }
        }
        System.out.println();
        if (!silent){
            System.out.println("------------------------------------------------------------" +
                               "-------------------");
            System.out.println(String.format("Selecting: %s\n With %d simulations.",
                                             bestMove,
                                             bestMove.getValue().n()));
            System.out.println("------------------------------------------------------------" +
                               "-------------------");

            System.out.println("--------------------------------"+
                               "Data Structures"+
                               "--------------------------------");
            System.out.println(String.format("Mast tables size: [%d, %d]",  mast.size(0),
                                                                            mast.size(1)));
            System.out.println("Dag connections made this turn: " + DagCounter);
            DagCounter = 0;
            System.out.println();
        }
        return bestMove.getKey();
    } //}}

    //public void applyMove(List<Move> moves)  {{
    /**
     * Updates the root node to the given moves
     *
     * @param moves The moves that need to be made
     */
    public void applyMove(List<Move> moves)  {
        if (!silent){
            System.out.println("The applied move !: " + moves.toString());
            System.out.println("Average playout depth: " + avgPlayOutDepth);
            System.out.println("Counter: " + counter);
        }
        synchronized(root){
            for (Map.Entry<List<Move>, RaveNode> entry: root.getChildren().entrySet()){
                if (moves.get(0).equals(entry.getKey().get(0)) &&
                        moves.get(1).equals(entry.getKey().get(1))){

                    markAndSweep(Integer.MAX_VALUE);
                    root = entry.getValue();
                    return;
                }
            }
        }
        throw new IllegalStateException("A move was selected that was not" +
                                        " one of the root node moves");
    }//}}
    //}}

    //----------------MCTS Helper functions---------------{{
    //private void updateAverageDepth(){{
    private void updateAverageDepth(){
        float rebuild = ((avgPlayOutDepth * playOutCount) + lastPlayOutDepth);
        avgPlayOutDepth = rebuild / (float)(playOutCount + 1);
        playOutCount++;
    }//}}

    //private void checkHeap(){{
    private void checkHeap(){
        if(((runtime.totalMemory() - runtime.freeMemory())/((float)runtime.maxMemory())) >= 0.85f){
            expanding = false;
        } else {
            expanding = true;
        }
    } //}}

    //private List<Double> getGoalsAsDouble(MachineState state)throws GoalDefinitionException{{
    private ArrayList<Double> getGoalsAsDouble(MachineState state)throws GoalDefinitionException{
            ArrayList<Double> result = new ArrayList<>();
            for(Integer inte : machine.getGoals(state)){
                result.add((double)inte);
            }
            return result;
    }//}}

    //private void applyDiscount(List<Double> lis, double discount){{
    private void applyDiscount(List<Double> lis, double discount){
        for(int i = 0; i < lis.size(); ++i){
            lis.set(i, lis.get(i) * discount);
        }
    }//}}

    //public String baseEval(){{
    public String baseEval(){
        String result = "";
        synchronized(root){
            DecimalFormat f = new DecimalFormat("#.##f");
            for(Map.Entry<List<Move>, RaveNode> entry : root.getChildren().entrySet()){
                result += "(";
                result += "m:" + entry.getKey();
                result += " n:" + entry.getValue().n();
                result += " v:[" + f.format(root.QValue(0, entry.getValue())) + " " +
                    f.format(root.QValue(1, entry.getValue())) + "]";
                result += ") ";
            }
        }
        return result;
    }//}}

    //public long size(){{
    /**
     * @return the size of the tree
     */
    public long size(){
        return root.size();
    }//}}

    //public void shutdown(){{
    /**
     * Breaks the searcher out of his loop
     */
    public void shutdown(){
        dag = new HashMap<>(100000);
        mast = new MAST(this.gamer.getMatch().getGame().getName());
    }//}}
    //}}
}

// vim: set foldmethod=marker:
// vim: set foldmarker={{,}}:
