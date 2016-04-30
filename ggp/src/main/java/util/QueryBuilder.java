package util;
import org.ggp.base.util.gdl.grammar.GdlSentence;
import org.ggp.base.util.gdl.grammar.GdlFunction;
import org.ggp.base.util.gdl.grammar.GdlTerm;
import org.ggp.base.util.gdl.grammar.GdlPool;


public class QueryBuilder {
    public static GdlSentence pieceCount(String where){
        GdlFunction fu = GdlPool.getFunction(GdlPool.getConstant(where),
                         new GdlTerm[]{GdlPool.getVariable("?a"), GdlPool.getVariable("?b")});
        return GdlPool.getRelation(GdlPool.TRUE, new GdlTerm[]{fu});
    }
}
