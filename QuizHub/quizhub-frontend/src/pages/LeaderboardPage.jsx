import { useEffect, useState } from "react";
import { getAllQuizzes } from "../services/quizService";
import { getQuizLeaderboard } from "../services/resultService";
import Navbar from "../components/Navbar";
import "../styles/LeaderboardPage.css";

export default function LeaderboardPage() {
  const [quizzes, setQuizzes] = useState([]);
  const [selectedQuiz, setSelectedQuiz] = useState("");
  const [leaderboard, setLeaderboard] = useState([]);
  const [timeFilter, setTimeFilter] = useState("all");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchQuizzes = async () => {
      try {
        const data = await getAllQuizzes();
        setQuizzes(data);
      } catch (err) {
        console.error("Failed to fetch quizzes:", err);
      }
    };
    fetchQuizzes();
  }, []);

  useEffect(() => {
    if (selectedQuiz) {
      fetchLeaderboard();
    }
  }, [selectedQuiz, timeFilter]);

  const fetchLeaderboard = async () => {
    if (!selectedQuiz) return;
    
    setLoading(true);
    try {
      const data = await getQuizLeaderboard(selectedQuiz, 10, timeFilter);
      setLeaderboard(data);
    } catch (err) {
      console.error("Failed to fetch leaderboard:", err);
    } finally {
      setLoading(false);
    }
  };

  const formatDuration = (durationString) => {
    const [hours, minutes, seconds] = durationString.split(':').map(Number);
    if (hours > 0) return `${hours}h ${minutes}m ${seconds}s`;
    if (minutes > 0) return `${minutes}m ${seconds}s`;
    return `${seconds}s`;
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString();
  };

  return (
    <div className="leaderboard-page">
      <Navbar />
      <div className="leaderboard-scroll-container">
        <div className="leaderboard-container">
          <h2>Quiz Leaderboard</h2>
          
          <div className="leaderboard-filters">
            <div className="filter-group">
              <label htmlFor="quiz-select">Select Quiz:</label>
              <select 
                id="quiz-select"
                value={selectedQuiz} 
                onChange={(e) => setSelectedQuiz(e.target.value)}
                className="quiz-dropdown"
              >
                <option value="">Choose a quiz...</option>
                {quizzes.map(quiz => (
                  <option key={quiz.id} value={quiz.id}>
                    {quiz.id} - {quiz.title}
                  </option>
                ))}
              </select>
            </div>

            <div className="filter-group">
              <label htmlFor="time-filter">Time Period:</label>
              <select 
                id="time-filter"
                value={timeFilter} 
                onChange={(e) => setTimeFilter(e.target.value)}
                className="time-dropdown"
              >
                <option value="all">All Time</option>
                <option value="month">Past Month</option>
                <option value="week">Past Week</option>
                <option value="day">Past Day</option>
              </select>
            </div>
          </div>

          <div className="leaderboard-content">
            {loading ? (
              <div className="loading">Loading leaderboard...</div>
            ) : selectedQuiz && leaderboard.length > 0 ? (
              <div className="leaderboard-list">
                <div className="leaderboard-header">
                  <span>Rank</span>
                  <span>User</span>
                  <span>Score</span>
                  <span>Duration</span>
                  <span>Completed</span>
                </div>
                
                <div className="leaderboard-scroll">
                  {leaderboard.map((entry, index) => (
                    <div 
                      key={entry.rank} 
                      className={`leaderboard-entry ${entry.isCurrentUser ? 'current-user' : ''} ${index < 3 ? `top-${index + 1}` : ''}`}
                    >
                      <span className="rank">
                        {entry.rank === 1 ? '🥇' : entry.rank === 2 ? '🥈' : entry.rank === 3 ? '🥉' : entry.rank}
                      </span>
                      <span className="username">{entry.username}</span>
                      <span className="score">{Math.round(entry.scorePercentage)}% ({entry.correctAnswers}/{entry.totalQuestions})</span>
                      <span className="duration">{formatDuration(entry.duration)}</span>
                      <span className="date">{formatDate(entry.completedAt)}</span>
                    </div>
                  ))}
                </div>
              </div>
            ) : selectedQuiz ? (
              <div className="no-results">No results found for this quiz and time period.</div>
            ) : (
              <div className="select-quiz">Please select a quiz to view the leaderboard.</div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}