package util;

import java.util.ArrayList;
import java.io.IOException;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.io.BufferedReader;
import java.io.InputStreamReader;

import org.ggp.base.util.symbol.factory.exceptions.SymbolFormatException;
import org.ggp.base.util.symbol.factory.SymbolFactory;
import org.ggp.base.util.symbol.grammar.Symbol;
import org.ggp.base.util.symbol.grammar.SymbolAtom;
import org.ggp.base.util.symbol.grammar.SymbolList;

import gamer.UnityGamer;

/**
 * Starts a bidirectional flow of information through a socket.
 *
 * Sends out the AI's evaluation of the board game.
 * Recieves parameters it uses to change how the AI plays.
 */
public final class Update extends Thread {
    private final int port;
    private final UnityGamer gamer;
    private ServerSocket listener;

    public Update(int port, UnityGamer gamer) throws IOException{
        System.out.println("Unity update constructor on port: " + port);
        while(listener == null) {
            try {
                listener = new ServerSocket(port, 0, InetAddress.getByName(null));
            } catch (IOException ex) {
                listener = null;
                port++;
                System.err.println("Failed to start gamer on port: " +
                        (port-1) + " trying port " + port);
            }
        }
        this.port = port;
        this.gamer = gamer;
    }

    public final int getUpdatePort(){
        return port;
    }


    @Override
    public void run(){
        System.out.println("Update Listener ready to recieve connections");
        try {
            Socket connection = listener.accept();
            PrintWriter pw = new PrintWriter(connection.getOutputStream(), true);
            BufferedReader inp = new BufferedReader(new InputStreamReader(connection.getInputStream()));
            while (listener != null) {
                String in;
                do {
                    in =  inp.readLine();
                } while(!in.toLowerCase().contains("ready"));
                while (listener != null) {
                    String eval = gamer.getEvaluation();
                    pw.println(eval);

                    String input  = inp.readLine();

                    if(input.toLowerCase().contains("abort")){
                        sleep(200);//Because windows sockets are garbage it seems
                        pw.println("ack");
                        System.out.println("Breaking out of update loop");
                        break;
                    } else if (input.toLowerCase().contains("update")) {
                        updateGamer(input);
                    }

                    sleep(200);
                }
            }
            connection.close();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    private void updateGamer(String input) throws SymbolFormatException {
        SymbolList list = (SymbolList) SymbolFactory.create(input);
        ArrayList<Double> controlValues = new ArrayList<>();
        for (int i = 1; i < list.size(); ++i){
            controlValues.add(Double.parseDouble(list.get(i).toString()));
        }
        gamer.updateValues(controlValues);
    }

    public void shutdown() {
        try {
            listener.close();
            listener = null;
        } catch (IOException e) {
            ;
        }
    }
}
