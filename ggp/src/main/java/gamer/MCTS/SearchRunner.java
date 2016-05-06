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
import com.google.common.cache.Cache;
import com.google.common.cache.CacheBuilder;


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
public abstract class SearchRunner extends Thread {
    //----------------MCTS Variables----------------------{{
    //General MCTS variables{{
    protected ReentrantReadWriteLock lock;
    protected StateMachine machine;
    protected StateMachineGamer gamer;
    public List<Move> newRoot;
    //}}
    //Extra info/control variables {{
    protected static Runtime runtime = Runtime.getRuntime();
    protected static boolean expanding;
    protected String gameName;

    public boolean silent;
    //}}
    //}}
    //public MCTSRAVE(StateMachineGamer gamer, ReentrantReadWriteLock lock,{{
    /**
     * Constructor for a MCTS class using GRAVE and MAST
     *
     * @param gamer             The gamer using this search
     * @param lock              A lock just to be safe
     * @param silent            Set to false to make it silent
     */
    public SearchRunner(StateMachineGamer gamer, ReentrantReadWriteLock lock,
                    boolean silent){
        this.gamer = gamer;
        this.lock = lock;
        this.machine = gamer.getStateMachine();
        this.newRoot = null;
        expanding = true;
        this.gameName = gamer.getMatch().getGame().getName();
        if(gameName == null){
            gameName = "Mylla";
        } else {
            System.out.println(gameName);
        }
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
        System.out.println("Using " + this.getClass().getName());
        while(!Thread.currentThread().isInterrupted()){
            try {
                limitWait();
                lock.writeLock().lock(); //Making sure the statemachine and tree are in sync
                if (newRoot != null){
                    applyMove(newRoot); //Move to our new root
                }
                if(heapCheck % 1000 == 0){
                    checkHeap();
                }
                heapCheck++;
                search();
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

    protected void printMemoryStats(){
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

    }
    protected abstract void printStats();
    protected abstract void printMoves();
    protected abstract void search() throws MoveDefinitionException,
                                            TransitionDefinitionException,
                                            GoalDefinitionException;
    protected abstract void limitWait() throws InterruptedException;
    protected abstract List<Move> bestMove() throws MoveDefinitionException;
    //public void applyMove(List<Move> moves)  {{
    /**
     * Should update the root node to the given moves
     *
     * @param moves The moves that need to be made
     */
    public abstract void applyMove(List<Move> moves);//}}
    //}}

    //}}


    //----------------MCTS Move managment-----------------{{
    //public List<Move> selectMove() throws MoveDefinitionException {{
    /**
     * @return The most simulated move at this point
     * @throws MoveDefinitionException Thrown in the GGP base
     */
    public List<Move> selectMove() throws MoveDefinitionException {
        List<Move> bestMove;
        if (!silent){
            printMemoryStats();
            System.out.println();
            printStats();
            System.out.println();
            printMoves();
            System.out.println();
        }
        bestMove = bestMove();
        System.out.println();
        return bestMove;
    } //}}


    //----------------Helper functions---------------{{

    //private void checkHeap(){{
    private void checkHeap(){
        if(((runtime.totalMemory() - runtime.freeMemory())/((float)runtime.maxMemory())) >= 0.85f){
            expanding = false;
        } else {
            expanding = true;
        }
    } //}}

    //private List<Double> getGoalsAsDouble(MachineState state)throws GoalDefinitionException{{
    protected ArrayList<Double> getGoalsAsDouble(MachineState state)throws GoalDefinitionException{
            ArrayList<Double> result = new ArrayList<>();
            for(Integer inte : machine.getGoals(state)){
                result.add((double)inte);
            }
            return result;
    }//}}

    //private void applyDiscount(List<Double> lis, double discount){{
    protected void applyDiscount(List<Double> lis, double discount){
        for(int i = 0; i < lis.size(); ++i){
            lis.set(i, lis.get(i) * discount);
        }
    }//}}

    //}}
}

// vim: set foldmethod=marker:
// vim: set foldmarker={{,}}:
