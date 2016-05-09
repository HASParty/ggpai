package util.requests;

import java.util.List;

import org.ggp.base.player.event.PlayerTimeEvent;
import org.ggp.base.player.gamer.Gamer;
import org.ggp.base.player.gamer.event.GamerUnrecognizedMatchEvent;
import org.ggp.base.player.gamer.exception.MoveSelectionException;
import org.ggp.base.player.request.grammar.Request;
import org.ggp.base.util.gdl.grammar.GdlTerm;
import org.ggp.base.util.logging.GamerLogger;
import org.ggp.base.util.statemachine.Move;
import org.ggp.base.util.statemachine.exceptions.MoveDefinitionException;

import gamer.UnityGamer;

public final class PullRequest extends Request {
    private final UnityGamer gamer;
    private final String matchId;

    public PullRequest(Gamer gamer, String matchId) {
        this.gamer = (UnityGamer) gamer;
        this.matchId = matchId;

    }

    @Override
    public String getMatchId() {
        return matchId;
    }

    @Override
    public String process(long receptionTime) {
        // First, check to ensure that this play request is for the match
        // we're currently playing. If we're not playing a match, or we're
        // playing a different match, send back "busy".
        if (gamer.getMatch() == null || !gamer.getMatch().getMatchId().equals(matchId)) {
            gamer.notifyObservers(new GamerUnrecognizedMatchEvent(matchId));
            GamerLogger.logError("GamePlayer", "Got play message not intended for current game: ignoring.");
            return "busy";
        }


        try {
            String move = gamer.selectMove(gamer.getMatch().getPlayClock()  + receptionTime).toString();
            if (move.contains(":")){
                String[] parts = move.split(":");
                move = parts[0] + ")" + ":" + parts[1].substring(0, parts[1].length() - 2);
            }
            String bob =  move + ":" + gamer.getLegalMoves(gamer.getOtherRole()).toString()
                   + ":" + gamer.getCurrentState().toString();
            System.err.println(bob);
            return bob;
        } catch (Exception e) {
            System.out.println(e.toString());
            GamerLogger.logStackTrace("GamePlayer", e);
            return "nil";
        }
    }

    @Override
    public String toString() {
        return "play";
    }
}
