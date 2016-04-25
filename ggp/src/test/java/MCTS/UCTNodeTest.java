package MCTS.test;

import java.util.ArrayList;
import java.util.List;
import java.util.Arrays;
import junit.framework.*;

import org.ggp.base.player.gamer.Gamer;
import org.ggp.base.player.gamer.exception.MetaGamingException;
import org.ggp.base.util.game.GameRepository;
import org.ggp.base.util.gdl.grammar.GdlPool;
import org.ggp.base.util.gdl.grammar.GdlTerm;
import org.ggp.base.util.match.Match;
import org.ggp.base.util.statemachine.Move;
import org.ggp.base.util.statemachine.Role;

import gamer.MCTS.nodes.UCTNode;

public class UCTNodeTest extends TestCase {
    protected UCTNode node;
    private static final double DELTA = 1e-15;

    protected void setUp(){
        ArrayList<Move> moves = new ArrayList<>();
        moves.add(Move.create("moveX"));
        moves.add(Move.create("moveY"));
        node = new UCTNode(moves);
    }

    public void testExpand(){
        ArrayList<List<Move>> moves = genMoves(0, 0);
        node.expand(moves);
        assertEquals(node.size(), 11);
        boolean there;
        for(List<Move> m: moves){
            there = false;
            for(List<Move> move: node.children.keySet()){
                if (move.equals(m)){
                    there = true;
                    break;
                }
            }
            assertTrue(there);
        }
    }

    public void testUpdate(){
        node.update(Arrays.asList(new Double[] {100.0d, 0.0d}));
        node.update(Arrays.asList(new Double[] {100.0d, 0.0d}));
        node.update(Arrays.asList(new Double[] {50.0d, 50.0d}));
        node.update(Arrays.asList(new Double[] {50.0d, 50.0d}));
        node.update(Arrays.asList(new Double[] {0.0d, 100.0d}));
        assertEquals(300, node.w(0), DELTA);
        assertEquals(200, node.w(1), DELTA);
        assertEquals(5, node.n());
    }

    private ArrayList<List<Move>> genMoves(int is, int js){
        ArrayList<List<Move>> moves = new ArrayList<>();
        String moveContents = "move";
        for (int i = is; i < 10 + is; i++){
            ArrayList<Move> tmp = new ArrayList<>();
            for (int j = js; j < 2 + js; j++){
                tmp.add(Move.create(moveContents + "." + i + "." + j));
            }
            moves.add(tmp);
        }
        return moves;
    }

}
