package gamer;
//Imports{{
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import org.ggp.base.apps.player.detail.DetailPanel;
import org.ggp.base.apps.player.detail.SimpleDetailPanel;
import org.ggp.base.player.gamer.exception.GamePreviewException;
import org.ggp.base.player.gamer.exception.MetaGamingException;
import org.ggp.base.player.gamer.exception.MoveSelectionException;
import org.ggp.base.player.gamer.statemachine.StateMachineGamer;
import org.ggp.base.util.prover.aima.AimaProver;
import org.ggp.base.util.game.Game;
import org.ggp.base.util.gdl.grammar.GdlTerm;
import org.ggp.base.util.logging.GamerLogger;
import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;
import org.ggp.base.util.statemachine.Role;
import org.ggp.base.util.statemachine.StateMachine;
import org.ggp.base.util.statemachine.cache.CachedStateMachine;
import org.ggp.base.util.statemachine.exceptions.GoalDefinitionException;
import org.ggp.base.util.statemachine.exceptions.MoveDefinitionException;
import org.ggp.base.util.statemachine.exceptions.TransitionDefinitionException;
import org.ggp.base.util.statemachine.implementation.prover.ProverStateMachine;
import is.ru.cadia.ggp.propnet.BackwardPropNetStateMachine;
import is.ru.cadia.ggp.propnet.structure.GGPBasePropNetStructureFactory;

import gamer.MCTS.MCTS;
import gamer.MCTS.MCTSControlValues;
import gamer.MCTS.MCTSRAVE;
//}}

/**
 * JeffGamer uses a simple MCTS search with UCT, GRAVE and MAST.
 *
 * Mainly used to test the algorithms used for the UnityGamer
 */

public class JeffGamer extends StateMachineGamer {
    //------------Variables----------------------------------------------------------------------{{
    private MCTSRAVE mcts;
    private Role other;
    private Map<Role, Integer> roleMap;
    private MCTSControlValues values;
    public AimaProver prover;
    public ReentrantReadWriteLock lock1= new ReentrantReadWriteLock(true);
    //}}

    //------------Initialize---------------------------------------------------------------------{{
    //public void stateMachineMetaGame(long timeout) {{
    @Override
    public void stateMachineMetaGame(long timeout) {
        prover = new AimaProver(getMatch().getGame().getRules());
        roleMap = getStateMachine().getRoleIndices();


        values = new MCTSControlValues();
        mcts = new MCTSRAVE(this, lock1, false, values);
        long finishBy = timeout - 1100;
        mcts.start();
        while(System.currentTimeMillis() < finishBy){
            try{
                Thread.sleep(200);
            } catch(Exception e){}//don't care
        }
    } //}}

    //public StateMachine getInitialStateMachine() {{
    // This is the default State Machine,
    @Override
    public StateMachine getInitialStateMachine() {
        return new BackwardPropNetStateMachine(new GGPBasePropNetStructureFactory());
    } //}}
    //}}

    //------------Move Selection-----------------------------------------------------------------{{
    //public GdlTerm selectMove(long timeout) throws MoveSelectionException {{
    @Override
    public GdlTerm selectMove(long timeout) throws MoveSelectionException {
        try {
            lock1.writeLock().lock();
            stateMachine.doPerMoveWork();

            List<GdlTerm> lastMoves = getMatch().getMostRecentMoves();
            if (lastMoves != null) {
                List<Move> moves = new ArrayList<Move>();
                for (GdlTerm sentence : lastMoves) {
                    moves.add(stateMachine.getMoveFromTerm(sentence));
                }

                currentState = stateMachine.getNextState(currentState, moves);
                mcts.newRoot = moves;

                getMatch().appendState(currentState.getContents());
            }
            Move move = stateMachineSelectMove(timeout);
            System.out.println("Picking move; " + move.toString());

            return move.getContents();
        }
        catch (Exception e) {
            System.out.println(e.toString());
            GamerLogger.logStackTrace("GamePlayer", e);
            throw new MoveSelectionException(e);
        }
    }//}}

    //public Move stateMachineSelectMove(long timeout) throws TransitionDefinitionException{{
    public Move stateMachineSelectMove(long timeout) throws TransitionDefinitionException,
                                                            MoveDefinitionException,
                                                            GoalDefinitionException {
        StateMachine theMachine = getStateMachine();
        long start = System.currentTimeMillis();
        long finishBy = timeout - 1100;
        int me = roleMap.get(getRole());
        lock1.writeLock().unlock();
        while (System.currentTimeMillis() < finishBy){
            sleep(50);
        }
        lock1.writeLock().lock();
        List<Move> li = mcts.selectMove();
        System.out.println(li.toString());
        lock1.writeLock().unlock();
        return li.get(roleMap.get(getRole()));
    } //}}
    //}}

    //------------End the gamer------------------------------------------------------------------{{
    //public void stateMachineStop() {{
    @Override
    public void stateMachineStop() {
        try{
            mcts.interrupt();
            mcts.join();
        } catch (Exception e){
            e.printStackTrace();
        }
        mcts = null;
        // Sample gamers do no special cleanup when the match ends normally.
    }//}}
    //public void stateMachineAbort() {{
    @Override
    public void stateMachineAbort() {
        try{
            mcts.interrupt();
            mcts.join();
        } catch (Exception e){
            e.printStackTrace();
        }
        mcts = null;
        // Sample gamers do no special cleanup when the match ends normally.
        // Sample gamers do no special cleanup when the match ends abruptly.
    } //}}
    //}}

    //------------Getters, Setters and helpers---------------------------------------------------{{
    //public String getName() {{
    @Override
    public String getName() {
        return "Jeff";
    }//}}

    //public final Role getOtherRole() {{
    /**
     * Returns the role that this gamer is playing as in the game.
     * @return Returns a role.
     */
    public final Role getOtherRole() {
        return other;
    } //}}

    //public List<Move> getLegalMoves(Role role) throws MoveDefinitionException{{
    public List<Move> getLegalMoves(Role role) throws MoveDefinitionException{
        if (role.equals(getRole())){
            return getStateMachine().getLegalMoves(getCurrentState(), getRole());
        } else if (role.equals(getOtherRole())) {
            return getStateMachine().getLegalMoves(getCurrentState(), getOtherRole());
        } else {
            return null;
        }
    } //}}

    //public String getEvaluation(){{
    public String getEvaluation(){
        return "[ Base:(" + mcts.baseEval() + ")]" ;
    } //}}

    //private void sleep(int ms){ //{{
    private void sleep(int ms){
        try{
            Thread.sleep(ms);
        } catch(Exception e){}//don't care

    }//}}
    //}}

    //------------Junk---------------------------------------------------------------------------{{
    //public void preview(Game g, long timeout) throws GamePreviewException {{
    @Override
    public void preview(Game g, long timeout) throws GamePreviewException {
        // Sample gamers do no game previewing.
    } //}}

    //public DetailPanel getDetailPanel() {{
    // This is the defaul Sample Panel
    @Override
    public DetailPanel getDetailPanel() {
        return new SimpleDetailPanel();
    } //}}
    //}}
}

// vim: set foldmethod=marker:
// vim: set foldmarker={{,}}:
