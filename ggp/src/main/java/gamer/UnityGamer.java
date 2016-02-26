package org.ggp.base.player.gamer.statemachine.unity;

import java.util.List;
import java.util.ArrayList;
import java.util.Map;

import org.ggp.base.player.gamer.exception.MetaGamingException;
import org.ggp.base.util.gdl.grammar.GdlConstant;
import org.ggp.base.util.statemachine.Role;
import org.ggp.base.apps.player.detail.DetailPanel;
import org.ggp.base.apps.player.detail.SimpleDetailPanel;
import org.ggp.base.player.gamer.exception.GamePreviewException;
import org.ggp.base.player.gamer.statemachine.StateMachineGamer;
import org.ggp.base.util.game.Game;
import org.ggp.base.util.logging.GamerLogger;
import org.ggp.base.util.statemachine.Move;
import org.ggp.base.player.gamer.statemachine.unity.MCTS.MCTS;
import org.ggp.base.util.gdl.grammar.GdlTerm;
import org.ggp.base.util.statemachine.implementation.propnet.forwardDeadReckon.ForwardDeadReckonPropnetStateMachine;
import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.StateMachine;
import org.ggp.base.util.statemachine.cache.CachedStateMachine;
import org.ggp.base.util.statemachine.exceptions.GoalDefinitionException;
import org.ggp.base.util.statemachine.exceptions.MoveDefinitionException;
import org.ggp.base.util.statemachine.exceptions.TransitionDefinitionException;
import org.ggp.base.util.statemachine.implementation.prover.ProverStateMachine;
import java.util.concurrent.locks.ReentrantReadWriteLock;
import org.ggp.base.player.gamer.exception.MoveSelectionException;

/**
 * UnityGamer is a special snowflake that does not work with a normal GGP server or the kiosk 
 */

public class UnityGamer extends StateMachineGamer
{
    protected MCTS mcts;
    private Role other;
    public boolean silent = false;
    public Map<Role, Integer> roleMap;
    //TODO: Swap this out for synchronizing on the root node
    public ReentrantReadWriteLock lock1= new ReentrantReadWriteLock(true);
    @Override
    public void stateMachineMetaGame(long timeout) {
        roleMap = getStateMachine().getRoleIndices();
        mcts = new MCTS(this, lock1, silent);
        long finishBy = timeout - 1000;
        mcts.start();
    }

    @Override
    public GdlConstant getRoleName(){
        String first = roleName.getValue();
        Role[] roles = stateMachine.getRoles();
        if (first.equals("first")){
            other = roles[0];
            return roles[1].getName();
        } else {
            other = roles[1];
            return roles[0].getName();
        }
    }

    @Override
    public String getName() {
        return "Unity";
    }

    @Override
    public StateMachine getInitialStateMachine() {
        return new ForwardDeadReckonPropnetStateMachine();
    }

    // This is the defaul Sample Panel
    @Override
    public DetailPanel getDetailPanel() {
        return new SimpleDetailPanel();
    }

    @Override
    public void stateMachineStop() {
        mcts.shutdown();
        if(!silent){
            System.out.println("Shutting down vie Stop");
        }
        try{
            mcts.join();
        } catch (Exception e){}
        mcts = null;
        // Sample gamers do no special cleanup when the match ends normally.
    }

    @Override
    public void stateMachineAbort() {
        mcts.shutdown();
        if(!silent){
            System.out.println("Shutting down vie Abort");
        }
        try{
            mcts.join();
        } catch (Exception e){}
        mcts = null;
        // Sample gamers do no special cleanup when the match ends abruptly.
    }

    @Override
    public void preview(Game g, long timeout) throws GamePreviewException {
        // Sample gamers do no game previewing.
    }


    /**
     * Applies the move recieved through a push request
     *
     * @return Null for non terminal states and won/draw/lost (applying to the other player) for terminals
     */
    public GdlTerm addMove(){
        lock1.writeLock().lock();
        stateMachine.doPerMoveWork();
        try{
            List<GdlTerm> lastMoves = getMatch().getMostRecentMoves();
            if (lastMoves != null) {
                List<Move> moves = new ArrayList<Move>();
                for (GdlTerm sentence : lastMoves) {
                    moves.add(stateMachine.getMoveFromTerm(sentence));
                }

                currentState = stateMachine.getNextState(currentState, moves);
                mcts.newRoot = moves;
                getMatch().appendState(currentState.getContents());
                if(stateMachine.isTerminal(currentState)){
                    int p = stateMachine.getGoal(currentState, getOtherRole());
                    if (p == 100){
                        return Move.create("won").getContents();
                    } else if (p > 1){
                        return Move.create("draw").getContents();
                    } else {
                        return Move.create("lost").getContents();
                    }
                }
            }
        } catch (Exception e){
            e.printStackTrace();
        }
        lock1.writeLock().unlock();
        return null;
    }

    @Override
    /**
     * @return The selected move + ""/Goal
     */
    public GdlTerm selectMove(long timeout) throws MoveSelectionException {
        lock1.writeLock().lock();
        try {
            List<Move> move = stateMachineSelectMoves(timeout);
            if (!silent){
                System.out.println("Picking move; " + move.toString());
            }
            currentState = stateMachine.getNextState(currentState, move);
            mcts.newRoot = move;

            getMatch().appendState(currentState.getContents());
            if(stateMachine.isTerminal(currentState)){
                int p = stateMachine.getGoal(currentState, getOtherRole());
                if (p == 100){
                    return Move.create(move.toString() + ":won").getContents();
                } else if (p > 1){
                    return Move.create(move.toString() + ":draw").getContents();
                } else {
                    return Move.create(move.toString() + ":lost").getContents();
                }
            }
        lock1.writeLock().unlock();
            return move.get(roleMap.get(getRole())).getContents(); 
        }
        catch (Exception e) {
            System.out.println(e.toString());
            GamerLogger.logStackTrace("GamePlayer", e);
            throw new MoveSelectionException(e);
        }
    }

    /**
     * @param role The role you want the moves for 
     * @return Legal moves for the given role
     */
    public List<Move> getLegalMoves(Role role) throws MoveDefinitionException{
        if (role.equals(getRole())){
            return getStateMachine().getLegalMoves(getCurrentState(), getRole());
        } else if (role.equals(getOtherRole())) {
            return getStateMachine().getLegalMoves(getCurrentState(), getOtherRole());
        } else {
            return null;
        }
    }

    /**
     * @return the role that this gamer is playing as in the game.
     */
    public final Role getOtherRole() {
        return other;
    }

    /**
     * @return the Evaluation of the players current state
     */
    public String getEvaluation(){
        return "";
    }

    /**
     * @param timeout isn't really used at this moment, might remove entirely
     * @return the current best move according to the MCTS class
     */
    public List<Move> stateMachineSelectMoves(long timeout) throws TransitionDefinitionException, MoveDefinitionException, GoalDefinitionException{

        StateMachine theMachine = getStateMachine();
        lock1.writeLock().lock();
        List<Move> li = mcts.selectMove();
        lock1.writeLock().unlock();
        return li;
    }

    /**
     * Not used in this implementation
     * @return Null
     */
    public Move stateMachineSelectMove(long timeout) throws TransitionDefinitionException, MoveDefinitionException, GoalDefinitionException{

        return null;
    }
}

