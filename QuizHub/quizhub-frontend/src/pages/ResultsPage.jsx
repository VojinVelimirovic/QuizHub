import { useEffect, useState, useContext } from "react";
import { getAllResults } from "../services/resultService";
import { AuthContext } from "../context/AuthContext";
import Navbar from "../components/Navbar";
import { useNavigate } from "react-router-dom";
import "../styles/ResultsPage.css";

export default function ResultsPage() {
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(true);
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();

  useEffect(() => {
    if (!user) {
      navigate("/login");
      return;
    }
    if (user.role !== "Admin") {
      navigate("/quizzes");
      return;
    }

    const fetchResults = async () => {
      try {
        const data = await getAllResults();
        setResults(data);
      } catch (err) {
        console.error("Failed to fetch results:", err);
      } finally {
        setLoading(false);
      }
    };
    fetchResults();
  }, [user, navigate]);

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

  if (loading) {
    return (
      <div className="results-page">
        <Navbar />
        <div className="results-container">
          <p>Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="results-page">
      <Navbar />
      <div className="results-scroll-container">
        <div className="results-container">
          <h2>All Results</h2>
          <div className="results-list">
            {results.length === 0 ? (
              <p className="no-results">No results found.</p>
            ) : (
              results.map((result, index) => (
                <div key={index} className="result-item">
                  <div className="result-header">
                    <h3>{result.quizTitle}</h3>
                    <span className="score">{Math.round(result.scorePercentage)}%</span>
                  </div>
                  <div className="result-details">
                    <p><strong>User:</strong> {result.username}</p>
                    <p><strong>Score:</strong> {result.score}/{result.totalQuestions}</p>
                    <p><strong>Completed:</strong> {formatLocalTime(result.completedAt)}</p>
                    <p><strong>Duration:</strong> {formatDuration(result.duration)}</p>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}