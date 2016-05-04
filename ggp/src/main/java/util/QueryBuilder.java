package util;

import java.util.List;
import java.util.ArrayList;
import org.ggp.base.util.gdl.grammar.GdlSentence;
import org.ggp.base.util.gdl.grammar.GdlFunction;
import org.ggp.base.util.gdl.grammar.GdlTerm;
import org.ggp.base.util.gdl.grammar.GdlPool;
import org.ggp.base.util.statemachine.Role;


public class QueryBuilder {
    public static ArrayList<GdlSentence> pieceCount(String where, List<Role> roles){
        ArrayList<GdlSentence> result = new ArrayList<>();
        for (Role s : roles){
            GdlFunction fu = GdlPool.getFunction(GdlPool.getConstant(where),
                             new GdlTerm[]{s.getName(), GdlPool.getVariable("?b")});
            result.add(GdlPool.getRelation(GdlPool.TRUE, new GdlTerm[]{fu}));
        }
        return result;
    }

}
