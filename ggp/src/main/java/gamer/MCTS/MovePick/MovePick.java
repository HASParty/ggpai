package gamer.MCTS.MovePick;
import java.util.List;
import org.ggp.base.util.statemachine.Move;

public abstract class MovePick{
    protected String gameName;
    public MovePick(String gameName){
        this.gameName = gameName;
    }
    public abstract List<Move> pickMove(List<List<Move>> list);
    public abstract void update(List<Move> moves, List<Double> newValue);
    public abstract int size(int which);
    public abstract void loadData();
    public abstract void saveData();

}
