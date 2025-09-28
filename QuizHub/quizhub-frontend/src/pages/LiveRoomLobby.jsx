import { useState, useEffect, useContext } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { liveRoomService } from "../services/liveRoomService";
import { signalRService } from "../services/signalRService";
import { AuthContext } from "../context/AuthContext";
import Navbar from "../components/Navbar";
import "../styles/LiveRoomLobby.css";

export default function LiveRoomLobby() {
  const { roomCode } = useParams();
  const navigate = useNavigate();
  const { user, token } = useContext(AuthContext);
  const [lobby, setLobby] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [timeUntilStart, setTimeUntilStart] = useState(0);
  const [isStarting, setIsStarting] = useState(false);
  const [navigatingToPlay, setNavigatingToPlay] = useState(false);

  useEffect(() => {
    if (!user) {
      navigate("/login");
      return;
    }

    let isMounted = true;

    const setup = async () => {
      try {
        const lobbyData = await liveRoomService.joinRoom(roomCode, token);
        if (!isMounted) return;

        setLobby(lobbyData);
        setTimeUntilStart(lobbyData.timeUntilStart);
        await setupSignalR();

        if (!isMounted) return;
        setLoading(false);
      } catch (err) {
        if (!isMounted) return;
        const errorMessage = err.response?.data?.error || err.message || 'Failed to join room';
        setError(errorMessage);
        setLoading(false);
      }
    };

    setup();

    return () => {
      isMounted = false;
      signalRService.setOnQuizStarted(null);
      signalRService.setOnLobbyStatus(null);
      signalRService.setOnPlayerJoined(null);
      signalRService.setOnPlayerLeft(null);
      signalRService.setOnError(null);
    };
  }, [roomCode, user, navigate, token]);

  const setupSignalR = async () => {
    signalRService.setOnQuizStarted(() => {
      if (navigatingToPlay) return;
      setNavigatingToPlay(true);
      navigate(`/live/${roomCode}/play`);
    });

    signalRService.setOnLobbyStatus((updatedLobby) => {
      setLobby(updatedLobby);
    });

    signalRService.setOnPlayerJoined((data) => {});

    signalRService.setOnPlayerLeft((userId) => {
      if (lobby && lobby.players) {
        setLobby(prev => ({
          ...prev,
          players: prev.players.filter(p => p.userId !== userId)
        }));
      }
    });

    signalRService.setOnError((err) => {});

    try {
      await signalRService.ensureConnectedAndJoin(roomCode);
    } catch (err) {}
  };

  const handleStartQuiz = async () => {
    if (isStarting || navigatingToPlay) return;

    setIsStarting(true);

    try {
      await signalRService.startQuiz();
      setNavigatingToPlay(true);
    } catch (err) {
      try {
        await liveRoomService.startRoom(roomCode, token);
        setNavigatingToPlay(true);
        navigate(`/live/${roomCode}/play`);
      } catch (apiErr) {
        setError(apiErr.message || 'Failed to start quiz');
        setIsStarting(false);
      }
    }
  };

  const handleLeaveRoom = async () => {
    try {
      await signalRService.leaveRoom(roomCode);
      await liveRoomService.leaveRoom(roomCode, token);
      navigate("/quizzes");
    } catch (err) {
      navigate("/quizzes");
    }
  };

  useEffect(() => {
    if (!lobby) return;

    const timer = setInterval(() => {
      setTimeUntilStart(prev => {
        if (prev <= 1) {
          clearInterval(timer);
          if (lobby.isHost && !isStarting && !navigatingToPlay) {
            handleStartQuiz();
          }
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => {
      clearInterval(timer);
    };
  }, [lobby, isStarting, navigatingToPlay]);

  if (loading) {
    return (
      <div className="live-room-lobby-page">
        <Navbar />
        <div className="page-scroll-container">
          <div className="lobby-container">
            <p>Loading room...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="live-room-lobby-page">
        <Navbar />
        <div className="page-scroll-container">
          <div className="lobby-container">
            <div className="error-state">
              <h3>Error</h3>
              <p>{error}</p>
              <button onClick={() => navigate("/quizzes")}>Back to Quizzes</button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="live-room-lobby-page">
      <Navbar />
      <div className="page-scroll-container">
        <div className="lobby-container">
          <div className="lobby-header">
            <h2>Room: {lobby.name}</h2>
            <span className="room-code">{lobby.roomCode}</span>
          </div>

          <div className="lobby-content">
            <div className="left-column">
              <div className="quiz-info">
                <h3>Quiz Information</h3>
                <p><strong>Title:</strong> {lobby.quizTitle}</p>
                <p><strong>Description:</strong> {lobby.quizDescription}</p>
                <p><strong>Difficulty:</strong> {lobby.difficulty}</p>
              </div>

              <div className="game-settings">
                <h3>Game Settings</h3>
                <p><strong>Time per question:</strong> {lobby.secondsPerQuestion} seconds</p>
                <p><strong>Starts in:</strong> {timeUntilStart} seconds</p>
                <p><strong>Your role:</strong> {lobby.isHost ? 'Host' : 'Player'}</p>
              </div>
            </div>

            <div className="right-column">
              <div className="scoring-rules">
                <h4>🎯 Scoring Rules</h4>
                <div className="rules-list">
                  <div className="rule-item">
                    <span className="rule-points">+10 pts</span>
                    <span className="rule-description">Correct answer</span>
                  </div>
                  <div className="rule-item">
                    <span className="rule-points">+5 pts</span>
                    <span className="rule-description">First blood (first correct answer)</span>
                  </div>
                  <div className="rule-item">
                    <span className="rule-points">+3 pts</span>
                    <span className="rule-description">Fast response (within first 1/3 of time)</span>
                  </div>
                  <div className="rule-total">
                    <span className="total-points">Max 18 pts per question!</span>
                  </div>
                </div>
              </div>

              <div className="game-tips">
                <h4>💡 Pro Tips</h4>
                <ul>
                  <li>Answer quickly for bonus points!</li>
                  <li>Be the first correct answer for "First Blood" bonus</li>
                  <li>Watch the live leaderboard to see your ranking</li>
                  <li>Questions auto-advance after time runs out</li>
                </ul>
              </div>
            </div>
          </div>

          <div className="players-section">
            <h3>Players ({lobby.currentPlayers}/{lobby.maxPlayers})</h3>
            <div className="players-list">
              {lobby.players && lobby.players.length > 0 ? (
                lobby.players.map((player, index) => (
                  <div key={index} className="player-card">
                    <span>{player.username || `User ${player.userId}`}</span>
                    <span className="join-time">
                      Joined {new Date(player.joinedAt).toLocaleTimeString()}
                    </span>
                  </div>
                ))
              ) : (
                <p>No players in the room</p>
              )}
            </div>
          </div>

          <div className="lobby-actions">
            <button onClick={handleLeaveRoom} className="leave-btn">
              Leave Room
            </button>
            {lobby.isHost && (
              <button 
                onClick={handleStartQuiz} 
                className="start-btn"
                disabled={isStarting || timeUntilStart <= 0 || navigatingToPlay || lobby.currentPlayers < 2}
              >
                {isStarting ? 'Starting...' : 
                 navigatingToPlay ? 'Starting Quiz...' : 
                 lobby.currentPlayers < 2 ? `Need ${2 - lobby.currentPlayers} more players` :
                 `Start Quiz Now (${timeUntilStart}s)`}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}