package util;

import java.io.IOException;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.io.BufferedReader;
import java.io.InputStreamReader;

import org.ggp.base.util.http.HttpReader;
import org.ggp.base.util.http.HttpWriter;

import gamer.UnityGamer;

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
                System.out.println("THIS IS THE MESSAGE ->>>>>>>>>>>>>>>>>" + in + 
                        "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<----- NOTICE ME SENPAI!!!!");
                while (listener != null) {
                    String eval = gamer.getEvaluation();
                    pw.println(eval);

                    String input  = inp.readLine();

                    if(input.toLowerCase().contains("abort")){
                        sleep(1);
                        pw.println("ack");
                        System.out.println("Breaking out of update loop");
                        break;
                    } else {
                        //Do shit with what we get
                    }

                    sleep(200);
                }
            }
            connection.close();
        } catch (Exception e) {
            e.printStackTrace();
        }
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
