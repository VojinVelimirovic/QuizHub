import { useEffect, useState, useContext } from "react";
import { getAllQuizzes, deleteQuiz } from "../services/quizService";
import { liveRoomService } from "../services/liveRoomService";
import { AuthContext } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import Navbar from "../components/Navbar";
import { FiEdit2, FiTrash2, FiUsers, FiClock } from "react-icons/fi";
import "../styles/Quizzes.css";

export default function Quizzes() {
  const { user, token } = useContext(AuthContext);
  const [quizzes, setQuizzes] = useState([]);
  const [liveRooms, setLiveRooms] = useState([]);
  const [filteredQuizzes, setFilteredQuizzes] = useState([]);
  const [categoryFilter, setCategoryFilter] = useState("");
  const [difficultyFilter, setDifficultyFilter] = useState("");
  const [keyword, setKeyword] = useState("");
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const interval = setInterval(() => {
      setLiveRooms(prev => prev.map(room => {
        if (!room.startsAt) return room;
        const secondsUntilStart = Math.max(
          0,
          Math.floor((new Date(room.startsAt + "Z") - new Date()) / 1000)
        );
        return { ...room, secondsUntilStart };
      }));
    }, 1000);

    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    const fetchQuizzes = async () => {
      try {
        const data = await getAllQuizzes();
        setQuizzes(data);
        setFilteredQuizzes(data);
        setLoading(false);
      } catch (err) {
        setLoading(false);
      }
    };

    const fetchLiveRooms = async () => {
      if (!user || !token) {
        setLiveRooms([]);
        return;
      }

      try {
        const roomsData = await liveRoomService.getActiveRooms(token);
        const updatedRooms = roomsData.map(room => ({
          ...room,
          secondsUntilStart: room.startsAt
            ? Math.max(0, Math.floor((new Date(room.startsAt + "Z") - new Date()) / 1000))
            : 0,
        }));
        setLiveRooms(updatedRooms);
      } catch (err) {
        setLiveRooms([]);
      }
    };

    fetchQuizzes();
    setTimeout(fetchLiveRooms, 100);
    const interval = setInterval(fetchLiveRooms, 10000);
    return () => clearInterval(interval);
  }, [user, token]);

  useEffect(() => {
    let filtered = quizzes;
    if (categoryFilter) filtered = filtered.filter(q => q.categoryName === categoryFilter);
    if (difficultyFilter) filtered = filtered.filter(q => q.difficulty === difficultyFilter);
    if (keyword) filtered = filtered.filter(q =>
      q.title.toLowerCase().includes(keyword.toLowerCase())
    );
    setFilteredQuizzes(filtered);
  }, [quizzes, categoryFilter, difficultyFilter, keyword]);

  const categories = [...new Set(quizzes.map(q => q.categoryName))];

  const handleDelete = async (e, id) => {
    e.stopPropagation();
    if (!window.confirm("Delete this quiz?")) return;
    try {
      await deleteQuiz(id, token);
      setQuizzes(prev => prev.filter(q => q.id !== id));
      setFilteredQuizzes(prev => prev.filter(q => q.id !== id));
    } catch (err) {
      alert("Delete failed");
    }
  };

  const handleEdit = (e, id) => {
    e.stopPropagation();
    navigate(`/update-quiz/${id}`);
  };

  const handleJoinRoom = (roomCode) => {
    navigate(`/live/${roomCode}`);
  };

  const handleCreateRoom = () => {
    navigate("/create-room");
  };

  if (loading) {
    return (
      <div className="quizzes-page">
        <Navbar />
        <div className="page-scroll-container">
          <div className="quizzes-container">
            <p>Loading quizzes...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="quizzes-page">
      <Navbar />
      <div className="page-scroll-container">
        <div className="quizzes-container">
          <div className="page-header">
            <h2>Quizzes</h2>
            {user?.role === "Admin" && (
              <div className="header-actions">
                <button onClick={handleCreateRoom} className="create-room-btn">
                  Create Live Room
                </button>
              </div>
            )}
          </div>

          {liveRooms.length > 0 && (
            <div className="live-rooms-section">
              <h3>Live Rooms</h3>
              <div className="live-rooms-grid">
                {liveRooms.map(room => (
                  <div key={room.roomCode} className="live-room-card">
                    <div className="live-room-header">
                      <h4>{room.name}</h4>
                      <span className="room-code">{room.roomCode}</span>
                    </div>
                    <div className="live-room-details">
                      <p className="quiz-title">{room.quizTitle}</p>
                      <div className="live-room-stats">
                        <span className="players-count">
                          <FiUsers /> {room.currentPlayers}/{room.maxPlayers}
                        </span>
                        <span className="time-remaining">
                          <FiClock /> Starts in {Math.floor(room.secondsUntilStart)}s
                        </span>
                      </div>
                    </div>
                    <button
                      onClick={() => handleJoinRoom(room.roomCode)}
                      className="join-room-btn"
                      disabled={room.hasStarted}
                    >
                      {room.hasStarted ? "Started" : "Join Room"}
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="quizzes-section">
            <div className="quiz-filters">
              <input
                type="text"
                placeholder="Search by keyword..."
                value={keyword}
                onChange={e => setKeyword(e.target.value)}
              />
              <select value={categoryFilter} onChange={e => setCategoryFilter(e.target.value)}>
                <option value="">All Categories</option>
                {categories.map(c => <option key={c} value={c}>{c}</option>)}
              </select>
              <select value={difficultyFilter} onChange={e => setDifficultyFilter(e.target.value)}>
                <option value="">All Difficulties</option>
                <option value="Easy">Easy</option>
                <option value="Medium">Medium</option>
                <option value="Hard">Hard</option>
              </select>
            </div>

            {filteredQuizzes.length === 0 ? (
              <p>No quizzes match the selected filters.</p>
            ) : (
              <div className="quiz-list">
                {filteredQuizzes.map(q => (
                  <div
                    key={q.id}
                    className="quiz-card clickable"
                    onClick={() => navigate(`/quiz/${q.id}`)}
                  >
                    <div className="card-controls">
                      {user?.role === "Admin" && (
                        <>
                          <button className="icon-btn" onClick={e => handleEdit(e, q.id)} title="Edit quiz">
                            <FiEdit2 color="#555" />
                          </button>
                          <button className="icon-btn" onClick={e => handleDelete(e, q.id)} title="Delete quiz">
                            <FiTrash2 color="#555" />
                          </button>
                        </>
                      )}
                    </div>

                    <h3>{q.title}</h3>
                    <p>{q.description}</p>
                    <div className="quiz-info">
                      <span>Questions: <br />{q.questionCount}</span>
                      <span>Difficulty: <br />{q.difficulty}</span>
                      <span>Time: <br />{q.timeLimitMinutes} min</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}