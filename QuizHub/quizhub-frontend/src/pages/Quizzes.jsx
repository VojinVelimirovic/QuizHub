import { useEffect, useState, useContext } from "react";
import { getAllQuizzes } from "../services/quizService";
import { AuthContext } from "../context/AuthContext";
import Navbar from "../components/Navbar";
import "../styles/Quizzes.css";

export default function Quizzes() {
  const { user } = useContext(AuthContext);
  console.log(user)
  const [quizzes, setQuizzes] = useState([]);

  useEffect(() => {
    const fetchQuizzes = async () => {
      try {
        const data = await getAllQuizzes();
        setQuizzes(data);
      } catch (err) {
        console.error(err);
      }
    };

    fetchQuizzes();
  }, []);

  return (
    <div className="quizzes-page">
      <Navbar />
      <div className="quizzes-container">
        <h2>Quizzes</h2>
        {quizzes.length === 0 ? (
          <p>No quizzes currently exist.</p>
        ) : (
          <div className="quiz-list">
            {quizzes.map(q => (
              <div key={q.id} className="quiz-card">
                <h3>{q.title}</h3>
                <p>{q.description}</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
