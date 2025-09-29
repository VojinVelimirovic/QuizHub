import { useState, useEffect, useContext } from "react";
import { useNavigate } from "react-router-dom";
import { liveRoomService } from "../services/liveRoomService";
import { getAllQuizzes } from "../services/quizService";
import { AuthContext } from "../context/AuthContext";
import Navbar from "../components/Navbar";
import "../styles/CreateRoomPage.css";
import { LiveRoomCreateRequest } from "../models/LiveRoom";

export default function CreateRoomPage() {
  const [formData, setFormData] = useState({
    name: '',
    quizId: '',
    maxPlayers: 4,
    secondsPerQuestion: 30,
    startDelaySeconds: 60
  });
  const [quizzes, setQuizzes] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { user, token } = useContext(AuthContext);

  useEffect(() => {
    if (!user || user.role !== "Admin") {
      navigate("/quizzes");
      return;
    }
    loadQuizzes();
  }, [user, navigate]);

  const loadQuizzes = async () => {
    try {
      const data = await getAllQuizzes();
      setQuizzes(data);
    } catch (err) {
      console.error('Failed to load quizzes:', err);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const payload = new LiveRoomCreateRequest(
        formData.name,
        Number(formData.quizId),
        formData.maxPlayers,
        formData.secondsPerQuestion,
        formData.startDelaySeconds
      );
      await liveRoomService.createRoom(payload, token);
      navigate("/quizzes");
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create room');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    setFormData(prev => ({
      ...prev,
      [e.target.name]: e.target.value
    }));
  };

  return (
    <div className="create-room-page">
      <Navbar />
      <div className="create-room-container">
        <h2>Create Live Room</h2>
        
        <form onSubmit={handleSubmit} className="create-room-form">
          <div className="form-group">
            <label>Room Name</label>
            <input
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              required
              maxLength={100}
            />
          </div>

          <div className="form-group">
            <label>Quiz</label>
            <select
              name="quizId"
              value={formData.quizId}
              onChange={handleChange}
              required
            >
              <option value="">Select a quiz</option>
              {quizzes.map(quiz => (
                <option key={quiz.id} value={quiz.id}>
                  {quiz.title} ({quiz.difficulty})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Max Players (2-20)</label>
            <input
              type="number"
              name="maxPlayers"
              value={formData.maxPlayers}
              onChange={handleChange}
              min="2"
              max="20"
              required
            />
          </div>

          <div className="form-group">
            <label>Seconds Per Question (10-120)</label>
            <input
              type="number"
              name="secondsPerQuestion"
              value={formData.secondsPerQuestion}
              onChange={handleChange}
              min="10"
              max="120"
              required
            />
          </div>

          <div className="form-group">
            <label>Start Delay Seconds (10-300)</label>
            <input
              type="number"
              name="startDelaySeconds"
              value={formData.startDelaySeconds}
              onChange={handleChange}
              min="10"
              max="300"
              required
            />
          </div>

          {error && <div className="error-message">{error}</div>}

          <div className="form-actions">
            <button type="button" onClick={() => navigate('/quizzes')} disabled={loading}>
              Cancel
            </button>
            <button type="submit" disabled={loading}>
              {loading ? 'Creating...' : 'Create Room'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}