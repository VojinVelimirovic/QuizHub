import { useEffect, useState, useContext } from "react";
import { getAllQuizzes } from "../services/quizService";
import { AuthContext } from "../context/AuthContext";
import Navbar from "../components/Navbar";

export default function Quizzes() {
  const { user } = useContext(AuthContext);
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
    <div>
      <Navbar />
      <h2>Quizzes</h2>
      {quizzes.length === 0 ? (
        <p>No quizzes currently exist.</p>
      ) : (
        <ul>
          {quizzes.map(q => (
            <li key={q.id}>
              {q.title} - {q.description}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
