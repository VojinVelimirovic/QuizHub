import { useEffect, useState, useContext } from "react";
import { getAllQuizzes, deleteQuiz } from "../services/quizService";
import { AuthContext } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import Navbar from "../components/Navbar";
import { FiEdit2, FiTrash2 } from "react-icons/fi";
import "../styles/Quizzes.css";

export default function Quizzes() {
  const { user, token } = useContext(AuthContext);
  const [quizzes, setQuizzes] = useState([]);
  const [filteredQuizzes, setFilteredQuizzes] = useState([]);
  const [categoryFilter, setCategoryFilter] = useState("");
  const [difficultyFilter, setDifficultyFilter] = useState("");
  const [keyword, setKeyword] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    const fetchQuizzes = async () => {
      try {
        const data = await getAllQuizzes();
        setQuizzes(data);
        setFilteredQuizzes(data);
      } catch (err) {
        console.error(err);
      }
    };
    fetchQuizzes();
  }, []);

  useEffect(() => {
    let filtered = quizzes;

    if (categoryFilter) {
      filtered = filtered.filter(q => q.categoryName === categoryFilter);
    }
    if (difficultyFilter) {
      filtered = filtered.filter(q => q.difficulty === difficultyFilter);
    }
    if (keyword) {
      filtered = filtered.filter(q =>
        q.title.toLowerCase().includes(keyword.toLowerCase())
      );
    }

    setFilteredQuizzes(filtered);
  }, [quizzes, categoryFilter, difficultyFilter, keyword]);

  const categories = [...new Set(quizzes.map(q => q.categoryName))];

  const handleDelete = async (e, id) => {
    e.stopPropagation();
    if (!window.confirm("Delete this quiz?")) return;
    try {
      await deleteQuiz(id, token || localStorage.getItem("token"));
      setQuizzes(prev => prev.filter(q => q.id !== id));
      setFilteredQuizzes(prev => prev.filter(q => q.id !== id));
    } catch (err) {
      console.error("Failed to delete quiz:", err);
      alert("Delete failed");
    }
  };

  const handleEdit = (e, id) => {
    e.stopPropagation();
    navigate(`/update-quiz/${id}`);
  };

  return (
    <div className="quizzes-page">
      <Navbar />
      <div className="quizzes-container">
        <h2>Quizzes</h2>
        <div className="quiz-filters">
          <input
            type="text"
            placeholder="Search by keyword..."
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
          />
          <select value={categoryFilter} onChange={(e) => setCategoryFilter(e.target.value)}>
            <option value="">All Categories</option>
            {categories.map(c => <option key={c} value={c}>{c}</option>)}
          </select>
          <select value={difficultyFilter} onChange={(e) => setDifficultyFilter(e.target.value)}>
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
                <div className="card-controls" style={{ position: "absolute", right: 10, top: 8, display: "flex", gap: 8 }}>
                  {user?.role === "Admin" && (
                    <>
                      <button className="icon-btn" onClick={(e) => handleEdit(e, q.id)} title="Edit quiz">
                        <FiEdit2 color="#555" />
                      </button>
                      <button className="icon-btn" onClick={(e) => handleDelete(e, q.id)} title="Delete quiz">
                        <FiTrash2 color="#555" />
                      </button>
                    </>
                  )}
                </div>

                <h3>{q.title}</h3>
                <p>{q.description}</p>
                <div className="quiz-info">
                  <span>Questions: <br/>{q.questionCount}</span>
                  <span>Difficulty: <br/>{q.difficulty}</span>
                  <span>Time: <br/>{q.timeLimitMinutes} min</span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
