import { useEffect, useState } from "react";
import { getUserResults } from "../services/resultService";
import Navbar from "../components/Navbar";
import { LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid, ResponsiveContainer } from "recharts";
import "../styles/ProfilePage.css";

export default function ProfilePage() {
  const [results, setResults] = useState([]);
  const [expandedResultId, setExpandedResultId] = useState(null);

  useEffect(() => {
    const fetchResults = async () => {
      try {
        const data = await getUserResults();
        setResults(data);
      } catch (err) {
        console.error("Failed to fetch user results:", err);
      }
    };
    fetchResults();
  }, []);

  const toggleExpand = (resultId) => {
    setExpandedResultId(prev => prev === resultId ? null : resultId);
  };

  const getResultId = (result) => {
    return `${result.quizId}-${result.completedAt}`;
  };

  const getProgressionDataUpToAttempt = (quizId, currentCompletedAt) => {
    return results
      .filter(r => r.quizId === quizId)
      .filter(r => new Date(r.completedAt) <= new Date(currentCompletedAt))
      .sort((a,b) => new Date(a.completedAt) - new Date(b.completedAt))
      .map((r, index) => ({
        attempt: index + 1,
        score: Math.round(r.scorePercentage),
        completedAt: new Date(r.completedAt).toLocaleDateString(),
        isCurrent: r.completedAt === currentCompletedAt
      }));
  };

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

  const formatLocalTime = (utcDate) => {
    const date = new Date(utcDate);
    date.setHours(date.getHours() + 2);
    return date.toLocaleString('en-GB', {
      day: '2-digit',
      month: '2-digit', 
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const displayUserAnswer = (qResult) => {
    if (qResult.userTextAnswer !== undefined && qResult.userTextAnswer !== null) {
      return qResult.userTextAnswer || '(No answer)';
    }
    
    if (qResult.selectedAnswerTexts && qResult.selectedAnswerTexts.length > 0) {
      return qResult.selectedAnswerTexts.join(', ');
    }
    
    if (qResult.selectedAnswerIds && qResult.selectedAnswerIds.length > 0) {
      return qResult.selectedAnswerIds.join(', ');
    }
    
    return '(No answer)';
  };

  const displayCorrectAnswers = (qResult) => {
    if (qResult.correctTextAnswer !== undefined && qResult.correctTextAnswer !== null) {
      return qResult.correctTextAnswer || 'N/A';
    }

    if (qResult.correctAnswerTexts && qResult.correctAnswerTexts.length > 0) {
      return qResult.correctAnswerTexts.join(', ');
    }
    
    if (qResult.correctAnswerIds && qResult.correctAnswerIds.length > 0) {
      return qResult.correctAnswerIds.join(', ');
    }
    
    return 'N/A';
  };

  return (
    <div className="profile-page">
      <Navbar />
      <div className="profile-container">
        <h2>Your Quiz Results</h2>
        <div className="profile-scroll-inner">
          {results.map(result => {
            const resultId = getResultId(result);
            return (
              <div key={resultId} className="quiz-result-card">
                <div className="result-summary" onClick={() => toggleExpand(resultId)}>
                  <h3>{result.quizTitle}</h3>
                  <p>
                    Score: {result.correctAnswers} / {result.totalQuestions} ({Math.round(result.scorePercentage)}%)
                  </p>
                  <p>Completed: {formatLocalTime(result.completedAt)}</p>
                  <button className="expand-btn">
                    {expandedResultId === resultId ? 'Hide Details' : 'View Details'}
                  </button>
                </div>

                {expandedResultId === resultId && (
                  <div className="result-details">
                    <p><strong>Duration:</strong> {formatDuration(result.duration)}</p>
                    <div className="questions-review">
                      {result.questionResults.map((qResult, index) => {
                        const correct = qResult.correctAnswerIds || [];
                        return (
                          <div key={qResult.questionId} className={`question-result ${qResult.isCorrect ? 'correct' : 'incorrect'}`}>
                            <p><strong>Q{index + 1}:</strong> {qResult.questionText}</p>
                            <p><strong>Your answer:</strong> {displayUserAnswer(qResult)}</p>
                            <p><strong>Correct answer(s):</strong> {displayCorrectAnswers(qResult)}</p>
                          </div>
                        );
                      })}
                    </div>

                    <div className="progression-graph">
                      <h4>Progression</h4>
                      <ResponsiveContainer width="100%" height={200}>
                        <LineChart data={getProgressionDataUpToAttempt(result.quizId, result.completedAt)}>
                          <XAxis dataKey="attempt" label={{ value: 'Attempt', position: 'insideBottomRight', offset: -5 }} />
                          <YAxis label={{ value: 'Score %', angle: -90, position: 'insideLeft' }} />
                          <Tooltip />
                          <CartesianGrid strokeDasharray="3 3" />
                          <Line type="monotone" dataKey="score" stroke="#8884d8" />
                        </LineChart>
                      </ResponsiveContainer>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}