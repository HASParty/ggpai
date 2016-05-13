package gamer.MCTS;
// Imports {{
import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.Deque;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Random;
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
/**
 * A simple MCTS Thread that defines a Monte carlo search algorithm for
 * a tree made up of RaveNode nodes.
 *
 * Its made to be extremelly modifiable with the MCTSControlValues struct.  It is not intended
 * to be truly competitive and makes some asumptions that makes it unable to play some games.
 * For instance it cannot play games with more than one player and it also cannot play games
 * with multiple players moving in the same turn.
 */
public class MCTSRAVE extends SearchRunner {
    //----------------MCTS Variables----------------------{{
    //MCTS Enhancement variables {{
    //MCTS DAG
    private HashMap<MachineState, RaveNode> dag;
    //MAST
    private MovePick mast;
    private int counter = 0;
    private ArrayList<GdlSentence> pcQuery;
    //Aggression cache
    //}}
    //General MCTS variables{{
    private Random rand = new Random();
    protected RaveNode root;
    //}}
    //Extra info/control variables {{
    private MCTSControlValues values;
    private int DagCounter = 0;
    private int lastPlayOutDepth;
    private int playOutCount;
    private float avgPlayOutDepth;
    private ArrayList<Integer> maxCounts;
    //}}
    //}}
    //public MCTSRAVE(StateMachineGamer gamer, ReentrantReadWriteLock lock,{{
    /**
     * Constructor for a MCTS class using GRAVE and MAST
     *
     * @param gamer             The gamer using this search
     * @param lock              A lock just to be safe
     * @param silent            Set to false to make it silent
     * @param values Holds several values that affect the way the algorithm runs
     */
    public MCTSRAVE(StateMachineGamer gamer, ReentrantReadWriteLock lock,
                    boolean silent, MCTSControlValues values){
        super(gamer, lock, silent);
        this.maxCounts = new ArrayList<>();
        maxCounts.add(9);
        maxCounts.add(9);
        this.values = values;
        this.root = new RaveNode(null);
        this.root.state = gamer.getCurrentState();

        this.lastPlayOutDepth = 0;
        this.playOutCount = 0;
        this.avgPlayOutDepth = 0;

        RaveNode.setRaveBias(values.rave);
        RaveNode.setGrave(values.grave);
        RaveNode.setExplorationFactor(values.explorationFactor);

        this.dag = new HashMap<>(10000);
        this.mast = new MAST(gameName);
        if (gameName.contains("Mylla") || gameName.contains("Checkers")){
            this.pcQuery = QueryBuilder.pieceCount("pieces_on_board", machine.getRoles());
        } else {
            this.pcQuery = null; //To make it work with other games for testing
        }

        //Debug
        this.silent = silent;
    }
    //}}

    //----------------MCTS selection phase----------------{{
    //protected void search() throws MoveDefinitionException,{{
    @Override
    protected void search() throws MoveDefinitionException,
                                   TransitionDefinitionException,
                                   GoalDefinitionException{
        if (values.changed){
            RaveNode.setRaveBias(values.rave);
            RaveNode.setGrave(values.grave);
            RaveNode.setExplorationFactor(values.explorationFactor);
        }
        search(root, null, new ArrayList<List<Move>>(), 0);
    }//}}

    //private List<Double> search(RaveNode node,{{
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
                node.goals = getGoalsAsDouble(state); // Decreasing terminal calls
                applyPersonalityBias(node.goals, state);
                node.terminal = true;
            }
            result = new ArrayList<>(node.goals);
        } else if (node.leaf()){
            if(expanding && (depth < values.horizon)){ // We don't expand if we have hit a resource limit
                node.expand(machine.getLegalJointMoves(state));
            }
            result = playOut(state, rave); // We do one playout
        } else {
            grave = node.updateGrave(grave); // We add to our grave values
            List<Move> ci = node.select(grave);
            HashMap<List<Move>, RaveNode> children = node.getChildren();
            RaveNode child =  children.get(ci);
            if(child.n() == 0){ // We only ever use the SM once for each state
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
    private ArrayList<Double> depthCharge(MachineState state,
                                     List<List<Move>> rave) throws GoalDefinitionException,
                                                                   MoveDefinitionException,
                                                                   TransitionDefinitionException {
        lastPlayOutDepth++; // too keep track of our depths
        ArrayList<Double> result;
        if(machine.isTerminal(state)){
            updateAverageDepth();
            result = getGoalsAsDouble(state);
            applyPersonalityBias(result, state);
        } else if (lastPlayOutDepth > values.chargeDepth){  //If we hit our depth limit
            updateAverageDepth();
            result = new ArrayList<>(values.chargeDefaults);
            applyPersonalityBias(result, state); //And apply bias to it
        } else {
            //If the state isn't terminal and we havn't hit a limit we
            //use MAST in an epsilon greedy manner to pick moves.
            //otherwise we fall back to random moves.
            List<List<Move>> moves = machine.getLegalJointMoves(state);
            List<Move> chosen;
            if(rand.nextFloat() >= values.epsilon){
                chosen = moves.get(rand.nextInt(moves.size()));
            } else {
                chosen = mast.pickMove(moves);
            }
            state = machine.getNextState(state, chosen);
            result = depthCharge(state, rave);
            rave.add(chosen); //Build up the rave list on the way down
            mast.update(chosen, result);
            applyDiscount(result, values.chargeDiscount);
        }
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
        System.out.println("\n");
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
        //Basically just a bfs to mark which children are reachable
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
        if(pcQuery != null){
            for (GdlSentence query : pcQuery){
                GdlSentence answer = ((UnityGamer)gamer).prover.askOne(query, state.getContents());
                res.add(Integer.parseInt(answer.get(0).toSentence().get(1).toString()));
            }
        }
        return res;
    } //}}

    //private void applyPersonalityBias(ArrayList<Double> goals, MachineState state){{
    private void applyPersonalityBias(ArrayList<Double> goals, MachineState state){
        ArrayList<Integer> counts = getPieceCount(state);
        if (counts.size() >= 2){
            int p1Counts = counts.get(0);
            int p2Counts = counts.get(1);
            maxCounts.set(0, Math.max(maxCounts.get(0), p1Counts));
            maxCounts.set(1, Math.max(maxCounts.get(1), p2Counts));
            int p1Max = maxCounts.get(0);
            int p2Max = maxCounts.get(1);
            // Defensiveness
            goals.set(0,
                      Math.max(goals.get(0) - ((p1Max - p1Counts) * values.defensiveness.get(0)),
                             0.0));
            goals.set(1,
                      Math.max(goals.get(1) - ((p2Max - p2Counts) * values.defensiveness.get(1)),
                               0.0));
            // Aggression
            goals.set(0,
                      Math.min(goals.get(0) + ((p2Max - p2Counts) * values.aggression.get(0)),
                               100.0));
            goals.set(1,
                      Math.min(goals.get(1) + ((p1Max - p1Counts) * values.aggression.get(1)),
                               100.0));
        }
    }//}}
    //}}

    //----------------MCTS Move managment-----------------{{
    //public List<Move> bestMove() throws MoveDefinitionException {{
    /**
     * @return The most simulated move at this point
     * @throws MoveDefinitionException Thrown in the GGP base
     */
    @Override
    public List<Move> bestMove() throws MoveDefinitionException {
        Map.Entry<List<Move>, RaveNode> bestMove = null;
        boolean whoops = false;
        if(rand.nextFloat() <= values.randErr){
            System.out.println("Whoops, selecting a random move");
            whoops = true;

        }
        for (Map.Entry<List<Move>, RaveNode> entry : root.getChildren().entrySet()){
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
        if (!silent){
            System.out.println("------------------------------------------------------------" +
                               "-------------------");
            System.err.println(String.format("Selecting: %s\n With %d simulations.",
                                             bestMove,
                                             bestMove.getValue().n()));
            System.out.println("------------------------------------------------------------" +
                               "-------------------");
        }
        return bestMove.getKey();
    } //}}

    //public void applyMove(List<Move> moves)  {{
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
            System.out.println("Counter: " + counter);
        }
        synchronized(root){
            for (Map.Entry<List<Move>, RaveNode> entry: root.getChildren().entrySet()){
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
        throw new IllegalStateException("A move was selected that was not" +
                                        " one of the root node moves");
    }//}}
    //}}

    //----------------MCTS Helper functions---------------{{
    //protected void printMoves(){{
    @Override
    protected void printMoves(){
        System.out.println("================================"+
                           "Available moves"+
                           "================================");
        System.out.println("N: " + root.n());
        for (Map.Entry<List<Move>, RaveNode> entry : root.getChildren().entrySet()){
            System.out.println(String.format(" Move: %-40s %-40s",
                                             entry.getKey(),
                                             entry.getValue()));
        }
    }//}}

    //protected void limitWait() throws InterruptedException{{
    @Override
    protected void limitWait() throws InterruptedException{
        if(values.limit > 0){
            while(root.n() > values.limit && newRoot == null){
                Thread.sleep(5);
            }
        }

    }//}}

    //protected void printStats(){{
    @Override
    protected void printStats(){
        System.out.println("--------------------------------"+
                           "Data Structures"+
                           "--------------------------------");
        System.out.println(String.format("Mast tables size: [%d, %d]",  mast.size(0),
                                                                        mast.size(1)));
        System.out.println("Dag connections made this turn: " + DagCounter);
        DagCounter = 0;

    }//}}

    //private void updateAverageDepth(){{
    private void updateAverageDepth(){
        float rebuild = ((avgPlayOutDepth * playOutCount) + lastPlayOutDepth);
        avgPlayOutDepth = rebuild / (float)(playOutCount + 1);
        playOutCount++;
    }//}}

    //public String baseEval(){{
    /**
     * Returns an evaluation of the running game as the AI sees it
     *
     * @return String containing the values of each move available at this point
     */
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
     * Doesn't really serve a purpose anymore, has sentimental value.
     */
    public void shutdown(){
        dag = null;
        mast = null;
    }//}}
    //}}
}

// vim: set foldmethod=marker:
// vim: set foldmarker={{,}}:
