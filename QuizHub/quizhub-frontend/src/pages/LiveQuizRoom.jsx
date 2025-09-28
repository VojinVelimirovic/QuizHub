import { useState, useEffect, useContext, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";
import Navbar from "../components/Navbar";
import LiveLeaderboard from "../components/LiveLeaderboard";
import { signalRService } from "../services/signalRService";
import "../styles/LiveQuizRoom.css";

export default function LiveQuizRoom() {
  const { roomCode } = useParams();
  const navigate = useNavigate();
  const { user } = useContext(AuthContext);
  
  // State
  const [currentQuestion, setCurrentQuestion] = useState(null);
  const [leaderboard, setLeaderboard] = useState(null);
  const [selectedAnswer, setSelectedAnswer] = useState(null);
  const [hasAnswered, setHasAnswered] = useState(false);
  const [timeRemaining, setTimeRemaining] = useState(0);
  const [quizState, setQuizState] = useState("loading");
  const [error, setError] = useState("");
  const [connectedAndJoined, setConnectedAndJoined] = useState(false);

  // Refs
  const timerRef = useRef(null);
  const currentQuestionRef = useRef(null);
  const isSubmittingRef = useRef(false);
  const quizStateRef = useRef(quizState);

  // Keep refs in sync
  useEffect(() => {
    currentQuestionRef.current = currentQuestion;
  }, [currentQuestion]);

  useEffect(() => {
    quizStateRef.current = quizState;
  }, [quizState]);

  // Timer cleanup
  useEffect(() => {
    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
      }
    };
  }, []);

  // Main connection effect
  useEffect(() => {
    if (!user) {
      navigate("/login");
      return;
    }

    let isMounted = true;

    const setupSignalRHandlers = () => {
      signalRService.setOnLeaderboardUpdated((lb) => {
        if (!isMounted) return;
        setLeaderboard(lb);
      });

      signalRService.setOnQuestionStarted((questionData) => {
        if (!isMounted) return;
        
        console.log("🔄 Resetting UI state for new question");
        
        // Reset all state for new question
        setCurrentQuestion(questionData);
        
        if (questionData.questionType === "MultipleChoice") {
          setSelectedAnswer([]);
        } else if (questionData.questionType === "FillInTheBlank") {
          setSelectedAnswer("");
        } else {
          setSelectedAnswer(null);
        }
        
        setHasAnswered(false);
        isSubmittingRef.current = false;
        setTimeRemaining(questionData.timeRemaining);
        setQuizState("playing");
        
        startTimer(questionData.timeRemaining);
      });

      signalRService.setOnQuestionEnded((questionId) => {
        if (!isMounted) return;
        
        if (timerRef.current) {
          clearInterval(timerRef.current);
          timerRef.current = null;
        }
        
        setQuizState("between");
      });

      signalRService.setOnQuizStarted(() => {
        if (!isMounted) return;
        setQuizState("loading");
      });

      signalRService.setOnAnswerSubmitted((questionId) => {
        if (!isMounted) return;
        
        const currentQ = currentQuestionRef.current;
        
        // Only process if this is for the current question and we're playing
        if (currentQ && questionId === currentQ.questionId && quizStateRef.current === "playing") {
          console.log("✅ Answer confirmed by server");
          setHasAnswered(true);
          isSubmittingRef.current = false;
        }
      });
      
      signalRService.setOnQuizEnded((finalLeaderboard) => {
        if (!isMounted) return;
        setLeaderboard(finalLeaderboard);
        setQuizState("ended");
      });

      signalRService.setOnError((err) => {
        if (!isMounted) return;
        setError(err.message || String(err));
      });
    };

    const connectToQuiz = async () => {
      try {
        setupSignalRHandlers();
        await signalRService.ensureConnectedAndJoin(roomCode);
        
        if (!isMounted) return;
        
        setConnectedAndJoined(true);
        setQuizState("loading");
        
      } catch (err) {
        if (!isMounted) return;
        setError(err.message || "Failed to connect to live quiz");
      }
    };

    connectToQuiz();

    return () => {
      isMounted = false;
      signalRService.setOnQuestionStarted(null);
      signalRService.setOnQuizStarted(null);
      signalRService.setOnQuizEnded(null);
      signalRService.setOnLeaderboardUpdated(null);
      signalRService.setOnAnswerSubmitted(null);
      signalRService.setOnError(null);
    };
  }, [roomCode, user, navigate]);

  const startTimer = (initial) => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
    }
    
    setTimeRemaining(initial);
    timerRef.current = setInterval(() => {
      setTimeRemaining((prev) => {
        if (prev <= 1) {
          clearInterval(timerRef.current);
          timerRef.current = null;
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  };

  const handleAnswerSelect = (answerId) => {
    if (hasAnswered || isSubmittingRef.current) return;

    if (currentQuestion?.questionType === "MultipleChoice") {
      setSelectedAnswer(prev => {
        const currentAnswers = prev || [];
        
        if (currentAnswers.includes(answerId)) {
          return currentAnswers.filter(id => id !== answerId);
        } else {
          return [...currentAnswers, answerId];
        }
      });
    } else {
      setSelectedAnswer(answerId);
    }
  };

  const handleTextAnswerChange = (text) => {
    if (hasAnswered || isSubmittingRef.current) return;
    setSelectedAnswer(text);
  };

  const isAnswerValid = () => {
    if (!selectedAnswer) return false;
    
    switch (currentQuestion?.questionType) {
      case "MultipleChoice":
        return Array.isArray(selectedAnswer) && selectedAnswer.length > 0;
      case "FillInTheBlank":
        return typeof selectedAnswer === "string" && selectedAnswer.trim().length > 0;
      default:
        return selectedAnswer != null;
    }
  };

  const handleSubmitAnswer = async () => {
    if (!currentQuestion || !isAnswerValid() || hasAnswered || isSubmittingRef.current) {
      return;
    }
    
    try {
      isSubmittingRef.current = true;
      
      let answerToSend = selectedAnswer;
      
      if (currentQuestion.questionType === "MultipleChoice") {
        if (!Array.isArray(selectedAnswer)) {
          answerToSend = [selectedAnswer];
        }
        answerToSend = answerToSend.sort((a, b) => a - b);
      } else if (typeof selectedAnswer === 'string' && !isNaN(selectedAnswer)) {
        answerToSend = parseInt(selectedAnswer, 10);
      }
      
      await signalRService.submitAnswer(currentQuestion.questionId, answerToSend);
      
      // Don't set hasAnswered here - wait for server confirmation
      
    } catch (err) {
      console.error("Submit answer failed:", err);
      isSubmittingRef.current = false;
      
      if (err.message.includes("Already submitted answer")) {
        setHasAnswered(true);
      } else {
        setError("Failed to submit answer: " + err.message);
      }
    }
  };

  const handleLeaveRoom = async () => {
    try {
      await signalRService.leaveRoom();
    } catch (e) {
      console.warn("Error leaving room:", e);
    }
    navigate("/quizzes");
  };

  const handleRetryConnection = () => {
    setError("");
    setConnectedAndJoined(false);
    setQuizState("loading");
    
    setTimeout(() => {
      signalRService.ensureConnectedAndJoin(roomCode).catch(console.error);
    }, 500);
  };

  const renderAnswerOptions = () => {
    if (!currentQuestion) return null;

    const isDisabled = hasAnswered || isSubmittingRef.current;

    switch (currentQuestion.questionType) {
      case "MultipleChoice":
        return (
          <div className="answer-options">
            {currentQuestion.answerOptions.map(opt => (
              <div
                key={opt.id}
                className={`answer-option-checkbox ${Array.isArray(selectedAnswer) && selectedAnswer.includes(opt.id) ? "selected" : ""}`}
                onClick={() => handleAnswerSelect(opt.id)}
              >
                <input
                  type="checkbox"
                  checked={Array.isArray(selectedAnswer) && selectedAnswer.includes(opt.id)}
                  readOnly
                  disabled={isDisabled}
                />
                <span>{opt.text}</span>
              </div>
            ))}
          </div>
        );

      case "FillInTheBlank":
        return (
          <div className="answer-input">
            <input
              type="text"
              value={selectedAnswer || ""}
              onChange={(e) => handleTextAnswerChange(e.target.value)}
              placeholder="Type your answer here..."
              disabled={isDisabled}
            />
          </div>
        );

      default:
        return (
          <div className="answer-options">
            {currentQuestion.answerOptions.map(opt => (
              <button
                key={opt.id}
                className={`answer-option ${selectedAnswer === opt.id ? "selected" : ""}`}
                onClick={() => handleAnswerSelect(opt.id)}
                disabled={isDisabled}
              >
                {opt.text}
              </button>
            ))}
          </div>
        );
    }
  };

  if (error) {
    return (
      <div className="live-quiz-room-page">
        <Navbar />
        <div className="quiz-container">
          <div className="error-state">
            <h3>Connection Error</h3>
            <p>{error}</p>
            <button onClick={handleRetryConnection} className="retry-btn">
              Retry Connection
            </button>
            <button onClick={() => navigate("/quizzes")}>Back to Quizzes</button>
          </div>
        </div>
      </div>
    );
  }

  if (!connectedAndJoined || quizState === "loading") {
    return (
      <div className="live-quiz-room-page">
        <Navbar />
        <div className="quiz-container">
          <div style={{ padding: 20, textAlign: 'center' }}>
            <h3>Connecting to live quiz…</h3>
            <p>Waiting for question to start...</p>
            <div className="loading-spinner"></div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="live-quiz-room-page">
      <Navbar />
      <div className="quiz-container">
        <div className="quiz-header">
          <h2>Live Quiz</h2>
          <div className="quiz-info">
            <span>Room: {roomCode}</span>
            <span>Question: {currentQuestion?.questionIndex}/{currentQuestion?.totalQuestions}</span>
            <span>Time: {timeRemaining}s</span>
          </div>
        </div>

        <div className="quiz-content">
          {quizState === "playing" && currentQuestion && (
            <div className="question-section">
              <div className="question-card">
                <h3>{currentQuestion.text}</h3>
                <div className="question-type-indicator">
                  Type: {currentQuestion.questionType}
                </div>
                
                {renderAnswerOptions()}

                <div className="question-actions">
                  <button 
                    onClick={handleSubmitAnswer} 
                    disabled={!isAnswerValid() || hasAnswered || isSubmittingRef.current} 
                    className="submit-btn"
                  >
                    {isSubmittingRef.current ? "Submitting..." : hasAnswered ? "Answer Submitted" : "Submit Answer"}
                  </button>
                  {hasAnswered && <div className="waiting-message">Waiting for other players...</div>}
                </div>
              </div>
            </div>
          )}

          {quizState === "between" && leaderboard && (
            <div className="between-questions">
              <h3>Question Complete!</h3>
              <LiveLeaderboard leaderboard={leaderboard} />
              <p>Next question starting soon...</p>
            </div>
          )}

          {quizState === "ended" && leaderboard && (
            <div className="quiz-ended">
              <h3>Quiz Finished!</h3>
              <LiveLeaderboard leaderboard={leaderboard} isFinal />
              <button onClick={handleLeaveRoom} className="leave-btn">Return to Quizzes</button>
            </div>
          )}
        </div>

        <div className="sidebar">
          <LiveLeaderboard leaderboard={leaderboard} isCompact />
        </div>
      </div>
    </div>
  );
}