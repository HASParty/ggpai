package MCTS;

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

import gamer.MCTS.MCMove;

public class MCMoveTest extends TestCase {
    protected MCMove node;

    protected void setUp(){
        ArrayList<Move> moves = new ArrayList<>();
        moves.add(Move.create("moveX"));
        moves.add(Move.create("moveY"));
        node = new MCMove(moves);
    }

    public void testExpand(){
        ArrayList<List<Move>> moves = genMoves(0, 0);
        node.expand(moves);
        assertEquals(MCMove.N, 1);
        assertEquals(node.size(), 11);
        boolean there;
        for(List<Move> m: moves){
            there = false;
            for(MCMove child: node.children){
                if (child.move.equals(m)){
                    there = true;
                    break;
                }
            }
            assertTrue(there);
        }
    }

    public void testUpdate(){
        node.update(Arrays.asList(new Integer[] {100, 0}));
        node.update(Arrays.asList(new Integer[] {100, 0}));
        node.update(Arrays.asList(new Integer[] {50, 50}));
        node.update(Arrays.asList(new Integer[] {50, 50}));
        node.update(Arrays.asList(new Integer[] {0, 100}));
        assertEquals(300, node.w(0));
        assertEquals(200, node.w(1));
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
