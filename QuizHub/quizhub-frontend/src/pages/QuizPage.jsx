import { useEffect, useState, useContext } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getQuizById, submitQuiz } from "../services/quizService";
import { AuthContext } from "../context/AuthContext";
import Navbar from "../components/Navbar";
import "../styles/QuizPage.css";

export default function QuizPage() {
  const { id } = useParams();
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();
  
  const [quiz, setQuiz] = useState(null);
  const [isTakingQuiz, setIsTakingQuiz] = useState(false);
  const [quizResult, setQuizResult] = useState(null);
  const [userAnswers, setUserAnswers] = useState({});
  const [timeRemaining, setTimeRemaining] = useState(0);
  const [timeElapsed, setTimeElapsed] = useState(0);
  const [timer, setTimer] = useState(null);

  const formatDuration = (durationString) => {
    const [hours, minutes, seconds] = durationString.split(':').map(Number);
    
    if (hours > 0) {
      return `${hours}h ${minutes}m ${seconds}s`;
    } else if (minutes > 0) {
      return `${minutes}m ${seconds}s`;
    } else {
      return `${seconds}s`;
    }
  };

  useEffect(() => {
    const fetchQuiz = async () => {
      try {
        const data = await getQuizById(id);
        setQuiz(data);
        setTimeRemaining(data.timeLimitMinutes * 60);
      } catch (err) {
        console.error("Failed to fetch quiz:", err);
      }
    };
    fetchQuiz();
  }, [id]);

  useEffect(() => {
    if (isTakingQuiz && timeRemaining > 0) {
      const timerId = setInterval(() => {
        setTimeRemaining(prev => {
          if (prev <= 1) {
            handleAutoSubmit();
            return 0;
          }
          return prev - 1;
        });
        setTimeElapsed(prev => prev + 1);
      }, 1000);
      setTimer(timerId);

      return () => clearInterval(timerId);
    }
  }, [isTakingQuiz, timeRemaining]);

  const startQuiz = () => {
    setIsTakingQuiz(true);
    setUserAnswers({});
    setTimeElapsed(0);
  };

  const handleAnswerChange = (questionId, answerIds = [], textAnswer = null) => {
    setUserAnswers(prev => ({
      ...prev,
      [questionId]: {
        selectedAnswerIds: answerIds,
        textAnswer: textAnswer
      }
    }));
  };

  const handleAutoSubmit = async () => {
    if (timer) clearInterval(timer);
    await submitAnswers();
  };

  const handleManualSubmit = async () => {
    if (timer) clearInterval(timer);
    await submitAnswers();
  };

  const submitAnswers = async () => {
  try {
    const answers = quiz.questions.map(question => {
      const userAnswer = userAnswers[question.id] || {};
      return {
        questionId: question.id,
        selectedAnswerIds: userAnswer.selectedAnswerIds || [],
        textAnswer: userAnswer.textAnswer || null
      };
    });

    const submissionData = {
      quizId: parseInt(id),
      answers: answers,
      durationSeconds: timeElapsed
    };

    const result = await submitQuiz(submissionData);
    setQuizResult(result);
    setIsTakingQuiz(false);
  } catch (err) {
    console.error("Failed to submit quiz:", err);
  }
};


  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  if (!quiz) {
    return (
      <div className="quiz-page">
        <Navbar />
        <div className="quiz-container">
          <p>Loading quiz...</p>
        </div>
      </div>
    );
  }

  if (quizResult) {
    return (
      <div className="quiz-page">
        <Navbar />
        <div className="quiz-container">
          <div className="quiz-result">
            <h2>Quiz Completed!</h2>
            <div className="result-summary">
              <h3>{quiz.title}</h3>
              <div className="score-display">
                <div className="score-circle">
                  <span className="score-percentage">{Math.round(quizResult.scorePercentage)}%</span>
                  <span className="score-text">
                    {quizResult.correctAnswers} / {quizResult.totalQuestions} Correct
                  </span>
                </div>
              </div>
              <div className="result-details">
                <p><strong>Time Taken:</strong> {formatDuration(quizResult.duration)}</p>
                <p><strong>Completed At:</strong> {new Date(quizResult.completedAt).toLocaleString()}</p>
              </div>
            </div>

            <div className="question-results">
              <h4>Question Review</h4>
              {quizResult.questionResults.map((qResult, index) => {
                const selected = qResult.selectedAnswerIds || [];
                const correct = qResult.correctAnswerIds || [];

                return (
                  <div key={qResult.questionId} className={`question-result ${qResult.isCorrect ? 'correct' : 'incorrect'}`}>
                    <div className="question-header">
                      <span className="question-number">Question {index + 1}</span>
                      <span className={`result-indicator ${qResult.isCorrect ? 'correct' : 'incorrect'}`}>
                        {qResult.isCorrect ? '✓ Correct' : '✗ Incorrect'}
                      </span>
                    </div>

                    <p className="question-text">{qResult.questionText}</p>

                    {qResult.userTextAnswer !== undefined && (
                      <div className="text-answer-review">
                        <p><strong>Your answer:</strong> {qResult.userTextAnswer || '(No answer)'}</p>
                        <p><strong>Correct answer:</strong> {qResult.correctTextAnswer}</p>
                      </div>
                    )}

                    {(selected.length > 0 || correct.length > 0) && (
                      <div className="multiple-choice-review">
                        {selected.length > 0 && (
                          <>
                            <p><strong>Your selection:</strong></p>
                            <div className="selected-answers">
                              {quiz.questions.find(q => q.id === qResult.questionId)?.answerOptions
                                .filter(opt => selected.includes(opt.id))
                                .map(opt => <span key={opt.id} className="user-answer">{opt.text}</span>)
                              }
                            </div>
                          </>
                        )}
                        {correct.length > 0 && (
                          <>
                            <p><strong>Correct answer(s):</strong></p>
                            <div className="correct-answers">
                              {quiz.questions.find(q => q.id === qResult.questionId)?.answerOptions
                                .filter(opt => correct.includes(opt.id))
                                .map(opt => <span key={opt.id} className="correct-answer">{opt.text}</span>)
                              }
                            </div>
                          </>
                        )}
                      </div>
                    )}
                  </div>
                );
              })}


            </div>

            <div className="result-actions">
              <button onClick={() => navigate('/quizzes')} className="btn-secondary">
                Back to Quizzes
              </button>
              <button onClick={() => window.location.reload()} className="btn-primary">
                Retake Quiz
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="quiz-page">
      <Navbar />
      <div className="quiz-scroll-container">
        <div className="quiz-container">
        {!isTakingQuiz ? (
          <div className="quiz-intro">
            <h2>{quiz.title}</h2>
            <p className="quiz-description">{quiz.description}</p>
            
            <div className="quiz-details">
              <div className="detail-item">
                <span className="label">Category:</span>
                <span className="value">{quiz.categoryName}</span>
              </div>
              <div className="detail-item">
                <span className="label">Difficulty:</span>
                <span className="value">{quiz.difficulty}</span>
              </div>
              <div className="detail-item">
                <span className="label">Time Limit:</span>
                <span className="value">{quiz.timeLimitMinutes} minutes</span>
              </div>
              <div className="detail-item">
                <span className="label">Questions:</span>
                <span className="value">{quiz.questions.length}</span>
              </div>
            </div>

            <div className="quiz-instructions">
              <h4>Instructions:</h4>
              <ul>
                <li>You have {quiz.timeLimitMinutes} minutes to complete the quiz</li>
                <li>Each question is worth 1 point</li>
                <li>You must get all parts of a question correct to receive points</li>
                <li>The timer starts when you click "Start Quiz"</li>
                <li>You can submit early by clicking "Submit Answers"</li>
              </ul>
            </div>

            <button onClick={startQuiz} className="btn-primary start-quiz-btn">
              Start Quiz
            </button>
          </div>
        ) : (
          <div className="quiz-taking">
            <div className="quiz-header">
              <h2>{quiz.title}</h2>
              <div className="quiz-timer">
                <span className={`time-remaining ${timeRemaining < 60 ? 'warning' : ''}`}>
                  Time: {formatTime(timeRemaining)}
                </span>
              </div>
            </div>

            <div className="questions-container">
              {quiz.questions.map((question, index) => (
                <div key={question.id} className="question-card">
                  <div className="question-header">
                    <h3>Question {index + 1}</h3>
                    <span className="question-type">{question.questionType}</span>
                  </div>
                  
                  <p className="question-text">{question.text}</p>

                  {question.questionType === 'FillInTheBlank' ? (
                    <div className="fill-in-answer">
                      <input
                        type="text"
                        placeholder="Type your answer here..."
                        value={userAnswers[question.id]?.textAnswer || ''}
                        onChange={(e) => handleAnswerChange(question.id, [], e.target.value)}
                        className="text-input"
                      />
                    </div>
                  ) : question.questionType === 'TrueFalse' ? (
                    <div className="answer-options">
                      <label className="option-label">
                        <input
                          type="radio"
                          name={`question-${question.id}`}
                          value="true"
                          checked={userAnswers[question.id]?.selectedAnswerIds.includes(
                            question.answerOptions.find(opt => opt.text.toLowerCase() === 'true')?.id
                          ) || false}
                          onChange={() => {
                            const trueOption = question.answerOptions.find(opt => 
                              opt.text.toLowerCase() === 'true' || opt.text.toLowerCase() === 'correct'
                            );
                            if (trueOption) {
                              handleAnswerChange(question.id, [trueOption.id]);
                            }
                          }}
                        />
                        <span className="option-text">True</span>
                      </label>
                      <label className="option-label">
                        <input
                          type="radio"
                          name={`question-${question.id}`}
                          value="false"
                          checked={userAnswers[question.id]?.selectedAnswerIds.includes(
                            question.answerOptions.find(opt => opt.text.toLowerCase() === 'false')?.id
                          ) || false}
                          onChange={() => {
                            const falseOption = question.answerOptions.find(opt => 
                              opt.text.toLowerCase() === 'false' || opt.text.toLowerCase() === 'incorrect'
                            );
                            if (falseOption) {
                              handleAnswerChange(question.id, [falseOption.id]);
                            }
                          }}
                        />
                        <span className="option-text">False</span>
                      </label>
                    </div>
                  ) : (
                    <div className="answer-options">
                      {question.answerOptions.map(option => (
                        <label key={option.id} className="option-label">
                          <input
                            type={question.questionType === 'MultipleChoice' ? 'checkbox' : 'radio'}
                            name={`question-${question.id}`}
                            value={option.id}
                            checked={userAnswers[question.id]?.selectedAnswerIds.includes(option.id) || false}
                            onChange={(e) => {
                              const currentAnswers = userAnswers[question.id]?.selectedAnswerIds || [];
                              let newAnswers;
                              
                              if (question.questionType === 'MultipleChoice') {
                                newAnswers = e.target.checked
                                  ? [...currentAnswers, option.id]
                                  : currentAnswers.filter(id => id !== option.id);
                              } else {
                                newAnswers = e.target.checked ? [option.id] : [];
                              }
                              
                              handleAnswerChange(question.id, newAnswers);
                            }}
                          />
                          <span className="option-text">{option.text}</span>
                        </label>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>

            <div className="quiz-actions">
              <button onClick={handleManualSubmit} className="btn-primary submit-btn">
                Submit Answers
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  </div>
  );
}