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
import org.ggp.base.util.game.Game;
import org.ggp.base.util.gdl.grammar.GdlConstant;
import org.ggp.base.util.gdl.grammar.GdlTerm;
import org.ggp.base.util.logging.GamerLogger;
import org.ggp.base.util.statemachine.MachineState;
import org.ggp.base.util.statemachine.Move;
import org.ggp.base.util.statemachine.Role;
import org.ggp.base.util.statemachine.StateMachine;
import org.ggp.base.util.prover.aima.AimaProver;
import org.ggp.base.util.statemachine.cache.CachedStateMachine;
import org.ggp.base.util.statemachine.exceptions.GoalDefinitionException;
import org.ggp.base.util.statemachine.exceptions.MoveDefinitionException;
import org.ggp.base.util.statemachine.exceptions.TransitionDefinitionException;
import org.ggp.base.util.statemachine.implementation.prover.ProverStateMachine;
import is.ru.cadia.ggp.propnet.BackwardPropNetStateMachine;
import is.ru.cadia.ggp.propnet.structure.GGPBasePropNetStructureFactory;

import gamer.MCTS.MCTSRAVE;
import gamer.MCTS.MCTSControlValues;
//}}
//
/**
 * UnityGamer is a special snowflake that does not work with a normal GGP server or the kiosk
 */

public class UnityGamer extends StateMachineGamer {
    //------------Variables----------------------------------------------------------------------{{
    protected MCTSRAVE mcts;
    private Role other;
    private MCTSControlValues values;
    public boolean silent = false;
    public Map<Role, Integer> roleMap;
    public AimaProver prover;
    //TODO: Swap this out for synchronizing on the root node
    public ReentrantReadWriteLock lock1= new ReentrantReadWriteLock(true);
    //}}

    //------------Initialize---------------------------------------------------------------------{{
    //public void stateMachineMetaGame(long timeout) {{
    @Override
    public void stateMachineMetaGame(long timeout) {
        prover = new AimaProver(getMatch().getGame().getRules());
        roleMap = getStateMachine().getRoleIndices();
        mcts = new MCTSRAVE(this, lock1, false, values);
        long finishBy = timeout - 1000;
        mcts.start();
    }//}}

    //public StateMachine getInitialStateMachine() {{
    @Override
    public StateMachine getInitialStateMachine() {
        return new BackwardPropNetStateMachine(new GGPBasePropNetStructureFactory());
    }//}}
    //}}

    //------------Move Selection-----------------------------------------------------------------{{
    //public List<Move> stateMachineSelectMoves(long timeout) throws TransitionDefinitionException, MoveDefinitionException, GoalDefinitionException{{
    /**
     * @param timeout isn't really used at this moment, might remove entirely
     * @return the current best move according to the MCTS class
     * @throws TransitionDefinitionException Thrown in the GGP base
     * @throws MoveDefinitionException Thrown in the GGP base
     * @throws GoalDefinitionException Thrown in the GGP base
     */
    public List<Move> stateMachineSelectMoves(long timeout) throws TransitionDefinitionException, MoveDefinitionException, GoalDefinitionException{

        StateMachine theMachine = getStateMachine();
        lock1.writeLock().lock();
        List<Move> li = mcts.selectMove();
        lock1.writeLock().unlock();
        return li;
    }//}}

    //public GdlTerm addMove(){{
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
    }//}}

    //public GdlTerm selectMove(long timeout) throws MoveSelectionException {{
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
    }//}}
    //}}

    //------------End the gamer------------------------------------------------------------------{{
    //public void stateMachineStop() {{
    @Override
    public void stateMachineStop() {
        mcts.interrupt();
        try{
            mcts.join();
        } catch (Exception e){}
        mcts = null;
        // Sample gamers do no special cleanup when the match ends normally.
    }//}}

    //public void stateMachineAbort() {{
    @Override
    public void stateMachineAbort() {
        mcts.interrupt();
        try{
            mcts.join();
        } catch (Exception e){}
        mcts = null;
        // Sample gamers do no special cleanup when the match ends abruptly.
    }//}}
    //}}

    //------------Getters, Setters and helpers---------------------------------------------------{{
    //public GdlConstant getRoleName(){{
    @Override
    public GdlConstant getRoleName(){
        StateMachine temp = new ProverStateMachine();
        temp.initialize(getMatch().getGame().getRules());
        String first = roleName.getValue();

        List<Role> roles = temp.getRoles();
        if (first.equals("first")){
            other = roles.get(0);
            return roles.get(1).getName();
        } else {
            other = roles.get(1);
            return roles.get(0).getName();
        }
    }//}}

    //public String getName() {{
    @Override
    public String getName() {
        return "Unity";
    }//}}

    //public void setValues(MCTSControlValues controlValues){{
    public void setValues(MCTSControlValues controlValues){
        this.values = controlValues;
    }//}}

   //public void updateValues(ArrayList<Double> controlValues){{
   public void updateValues(ArrayList<Double> controlValues){
       values.setAll(controlValues);
   } //}}

    //public String getEvaluation(){{
    /**
     * @return the Evaluation of the players current state
     */
    public String getEvaluation(){
        return "[ Base:(" + mcts.baseEval() + ")]" ;
    }//}}

    //public List<Move> getLegalMoves(Role role) throws MoveDefinitionException{{
    /**
     * @param role The role you want the moves for
     * @return Legal moves for the given role
     * @throws MoveDefinitionException Thrown in the GGP base
     */
    public List<Move> getLegalMoves(Role role) throws MoveDefinitionException{
        lock1.writeLock().lock();
        List<Move> res;
        if (getStateMachine().isTerminal(getCurrentState())){
            res = new ArrayList<>();
            res.add(Move.create("noop"));
            res.add(Move.create("noop"));
        } else {
            if (role.equals(getRole())){
                res =  getStateMachine().getLegalMoves(getCurrentState(), getRole());
            } else if (role.equals(getOtherRole())) {
                res = getStateMachine().getLegalMoves(getCurrentState(), getOtherRole());
            } else {
                res = null;
            }
        }
        lock1.writeLock().unlock();
        return res;
    }//}}

    //public final Role getOtherRole() {{
    /**
     * @return the role that this gamer is playing as in the game.
     */
    public final Role getOtherRole() {
        return other;
    }//}}
    //}}

    //------------Junk---------------------------------------------------------------------------{{
    //{{
    //public DetailPanel getDetailPanel() {{
    // This is the defaul Sample Panel
    @Override
    public DetailPanel getDetailPanel() {
        return new SimpleDetailPanel();
    }//}}
    //public void preview(Game g, long timeout) throws GamePreviewException {{
    @Override
    public void preview(Game g, long timeout) throws GamePreviewException {
        // Sample gamers do no game previewing.
    }//}}
    //public Move stateMachineSelectMove(long timeout) throws TransitionDefinitionException, MoveDefinitionException, GoalDefinitionException{{
    /**
     * Not used in this implementation
     * @return Null
     */
    public Move stateMachineSelectMove(long timeout) throws TransitionDefinitionException, MoveDefinitionException, GoalDefinitionException{

        return null;
    }//}}
    //}}}}
}

