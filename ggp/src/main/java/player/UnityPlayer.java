package org.ggp.base.player;

import java.io.IOException;
import java.lang.InterruptedException;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.ArrayList;
import java.util.List;

import org.ggp.base.player.event.PlayerDroppedPacketEvent;
import org.ggp.base.player.event.PlayerReceivedMessageEvent;
import org.ggp.base.player.event.PlayerSentMessageEvent;
import org.ggp.base.player.gamer.Gamer;
import org.ggp.base.player.gamer.statemachine.random.RandomGamer;
import org.ggp.base.player.request.factory.exceptions.RequestFormatException;
import org.ggp.base.player.request.grammar.Request;
import org.ggp.base.util.gdl.factory.GdlFactory;
import org.ggp.base.util.gdl.grammar.GdlConstant;
import org.ggp.base.util.http.HttpReader;
import org.ggp.base.util.http.HttpWriter;
import org.ggp.base.util.logging.GamerLogger;
import org.ggp.base.util.match.Match;
import org.ggp.base.util.observer.Event;
import org.ggp.base.util.observer.Observer;
import org.ggp.base.util.observer.Subject;
import org.ggp.base.util.symbol.factory.SymbolFactory;
import org.ggp.base.util.symbol.grammar.Symbol;
import org.ggp.base.util.symbol.grammar.SymbolAtom;
import org.ggp.base.util.symbol.grammar.SymbolList;

import gamer.UnityGamer;
import util.Update;
import util.requests.UnityRequest;
import util.requests.factory.UnityRequestFactory;


public class UnityPlayer extends GamePlayer {
    protected final Thread update; //For a bidirectional stream of information for the gamer
    public UnityPlayer(int port, Gamer gamer) throws IOException {
        super(port, gamer);
        this.update = new Update(9149, (UnityGamer)gamer);
        System.out.println("UnityPlayer ready to start metagaming");
    }

    @Override
    public void run() {
        boolean debug = false;
        update.start();
        while (listener != null) {
            try {
                Socket connection = listener.accept();
                String in = HttpReader.readAsServer(connection);
                if (in.length() == 0) {
                    throw new IOException("Empty message received.");
                }
                System.out.println("Message: " + in);

                GamerLogger.log("GamePlayer", "[Received at " +
                        System.currentTimeMillis() +
                        "] " + in, GamerLogger.LOG_LEVEL_DATA_DUMP);

                Request request = new UnityRequestFactory().create(gamer, in);
                String out = request.process(System.currentTimeMillis());

                HttpWriter.writeAsServer(connection, out);

                connection.close();
                GamerLogger.log("GamePlayer", "[Sent at " +
                        System.currentTimeMillis() + "] " +
                        out, GamerLogger.LOG_LEVEL_DATA_DUMP);
            } catch (Exception e) {
                System.err.println(e.toString());
                GamerLogger.log("GamePlayer", "[Dropped data at " +
                        System.currentTimeMillis() +
                        "] Due to " + e, GamerLogger.LOG_LEVEL_DATA_DUMP);
            }
        }
        try {
            update.join();
        } catch (InterruptedException e){
            System.out.println(e);
        }

    }
}
