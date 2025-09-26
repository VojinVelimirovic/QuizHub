import React, { useContext } from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Quizzes from "./pages/Quizzes";
import CreateQuizPage from "./pages/CreateQuizPage";
import { AuthContext } from "./context/AuthContext";
import QuizPage from "./pages/QuizPage";
import ProfilePage from "./pages/ProfilePage";
import LeaderboardPage from "./pages/LeaderboardPage";
import UpdateQuizPage from "./pages/UpdateQuizPage";

function App() {
  const { user } = useContext(AuthContext);

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route
          path="/quizzes"
          element={user ? <Quizzes /> : <Navigate to="/login" />}
        />
        <Route
          path="/quiz/:id"
          element={user ? <QuizPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/update-quiz/:id"
          element={user ? <UpdateQuizPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/profile"
          element={user ? <ProfilePage/> : <Navigate to="/login"/>}
        />
        <Route
          path="/leaderboard"
          element={user ? <LeaderboardPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/create-quiz"
          element={
            !user ? (
              <Navigate to="/login" />
            ) : user.role === "Admin" ? (
              <CreateQuizPage />
            ) : (
              <Navigate to="/quizzes" />
            )
          }
        />
        <Route
          path="*"
          element={<Navigate to={user ? "/quizzes" : "/login"} />}
        />
      </Routes>
    </Router>
  );
}

export default App;