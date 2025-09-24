import { useContext } from "react";
import { AuthContext } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import { FiLogOut, FiPlus } from "react-icons/fi";
import "../styles/Navbar.css";

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

  return (
    <nav className="navbar">
      <div className="profile-section">
        <div className="profile" onClick={handleProfileClick}>
          <img src={`https://localhost:7208${user?.profilePictureUrl}` || "/default-profile.png"} alt="Profile" />
          <span>{user?.username}</span>
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
