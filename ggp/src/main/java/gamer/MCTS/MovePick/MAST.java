package gamer.MCTS.MovePick;

import java.util.HashMap;
import java.util.Map;
import java.util.List;
import java.util.Random;
import java.util.ArrayList;
import java.io.File;
import java.io.ObjectOutputStream;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.ObjectInputStream;
import org.ggp.base.util.statemachine.Move;

public class MAST extends MovePick{
    private ArrayList<HashMap<Move, List<Double>>> mast; //TODO split this on roles.
    private Random rand;
    public MAST(String gameName){
        super(gameName);
        mast = new ArrayList<>();
        mast.add(new HashMap<>());
        mast.add(new HashMap<>());
        rand = new Random();

    }
    @Override
    public List<Move> pickMove(List<List<Move>> list){
        Move[] bestMoves = new Move[list.get(0).size()];
        double[] bestValue = new double[list.get(0).size()];
        for (List<Move> moves : list){
            for (int i = 0; i < moves.size(); ++i){
                double mastValue;
                if (mast.get(i).containsKey(moves.get(i))){
                    mastValue = mast.get(i).get(moves.get(i)).get(0);
                    mastValue += rand.nextFloat() * 5;
                } else {
                    mastValue = rand.nextInt(30000);  //Arbitrary random value
                }
                if((bestMoves[i] == null) || (bestValue[i] > mastValue)){
                    bestMoves[i] = moves.get(i);
                    bestValue[i] = mastValue;
                }
            }
        }
        for(List<Move> moves : list){
            boolean best = true;
            for (int i = 0; i < moves.size(); ++i){
                if(!moves.get(i).equals(bestMoves[i])){
                    best = false;
                }
            }
            if(best){
                return moves;
            }
        }
        return null; //Something got fucked up
    }
    @Override
    public void update(List<Move> moves, List<Double> newValue){
        for(int i = 0; i < newValue.size(); i++){
            Move move = moves.get(i);
            List<Double> newList;
            if(mast.get(i).containsKey(move)){
                newList = mast.get(i).get(move);
                int oldCount = (int)Math.round(newList.get(1));
                int newCount = oldCount + 1;
                newList.set(0, ((newList.get(0) * oldCount) + newValue.get(i))/newCount);
                newList.set(1, (double)newCount);
            } else {
                newList = new ArrayList<>();
                newList.add(newValue.get(i));
                newList.add(1.0d);
                mast.get(i).put(move, newList);
            }
        }
    }
    @Override
    public int size(int which){
        return mast.get(which).size();
    }

    @SuppressWarnings("unchecked")
    @Override
    public void loadData(){
        File file = new File("data/mast/" + gameName);
        if(!file.isFile()){
            return;
        }
        try{
            FileInputStream fis = new FileInputStream(file);
            ObjectInputStream ois = new ObjectInputStream(fis);
            mast = (ArrayList) ois.readObject();
            fis.close();
            ois.close();
        } catch (Exception e){
            System.out.println("EXCEPTION: " + e.toString());
            e.printStackTrace();
        }
    }

    @Override
    public void saveData(){
        File file = new File("data/mast/" + gameName);
        try{
            FileOutputStream fos = new FileOutputStream(file);
            ObjectOutputStream oos = new ObjectOutputStream(fos);
            oos.writeObject(mast);
            fos.close();
            oos.close();
        } catch (Exception e){
            System.out.println("EXCEPTION: " + e.toString());
            e.printStackTrace();
        }
    }
}
