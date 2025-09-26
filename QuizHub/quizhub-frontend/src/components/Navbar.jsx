import { useContext } from "react";
import { AuthContext } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import { FiLogOut, FiPlus, FiBarChart2 } from "react-icons/fi";
import "../styles/Navbar.css";
import logo from "../assets/quizhub-logo.png";

export default function Navbar() {
  const { user, logoutUser } = useContext(AuthContext);
  const navigate = useNavigate();

  const handleLogout = () => {
    logoutUser();
    navigate("/login");
  };

  const handleProfileClick = () => {
    navigate("/profile");
  };

  const handleCreateQuiz = () => {
    navigate("/create-quiz");
  };

  const handleLeaderboardClick = () => {
    navigate("/leaderboard");
  };

  return (
    <nav className="navbar">
      <div className="navbar-left">
        <img
          src={logo}
          alt="QuizHub Logo"
          className="navbar-logo"
          onClick={() => navigate("/quizzes")}
        />
        <div className="profile" onClick={handleProfileClick}>
          <img src={`https://localhost:7208${user?.profilePictureUrl}` || "/default-profile.png"} alt="Profile" />
          <span>{user?.username}</span>
        </div>
        
        <div className="leaderboard-nav" onClick={handleLeaderboardClick}>
          <FiBarChart2 size={18} />
          <span>Leaderboard</span>
        </div>
        
        {user?.role === "Admin" && (
          <button onClick={handleCreateQuiz} className="create-quiz-btn" title="Create Quiz">
            <FiPlus size={18} />
          </button>
        )}
      </div>
      <button onClick={handleLogout} className="logout-btn" title="Logout">
        <FiLogOut size={18} />
      </button>
    </nav>
  );
}