import "../styles/LiveLeaderboard.css";

export default function LiveLeaderboard({ leaderboard, isCompact = false, isFinal = false }) {
  if (!leaderboard || !leaderboard.entries) {
    return <div className="leaderboard-loading">Loading leaderboard...</div>;
  }

  // Handle empty leaderboard
  if (leaderboard.entries.length === 0) {
    return (
      <div className={`leaderboard ${isCompact ? 'compact' : ''}`}>
        <h3>{isFinal ? 'Final Results' : 'Live Leaderboard'}</h3>
        <div className="no-players">No players in the leaderboard</div>
      </div>
    );
  }

  return (
    <div className={`leaderboard ${isCompact ? 'compact' : ''}`}>
      <h3>{isFinal ? 'Final Results' : 'Live Leaderboard'}</h3>
      
      <div className="leaderboard-list">
        {leaderboard.entries.map((entry, index) => (
          <div key={index} className={`leaderboard-entry ${index < 3 ? `top-${index + 1}` : ''}`}>
            <div className="entry-position">#{entry.position}</div>
            <div className="entry-user">
              <span className="username">{entry.username}</span>
              {isCompact && <span className="score">{entry.score} pts</span>}
            </div>
            {!isCompact && (
              <div className="entry-details">
                <span>Score: {entry.score}</span>
                <span>Correct: {entry.correctAnswers}</span>
                <span>Avg Time: {entry.averageResponseTime.toFixed(1)}s</span>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}