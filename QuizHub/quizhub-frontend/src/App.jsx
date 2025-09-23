import React, { useContext } from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Quizzes from "./pages/Quizzes";
import CreateQuizPage from "./pages/CreateQuizPage";
import { AuthContext } from "./context/AuthContext";

function App() {
  const { user } = useContext(AuthContext);
  console.log(user)

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* Quizzes accessible to any authenticated user */}
        <Route
          path="/quizzes"
          element={user ? <Quizzes /> : <Navigate to="/login" />}
        />

        {/* CreateQuizPage only for admin users */}
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

        {/* Catch-all redirects */}
        <Route
          path="*"
          element={<Navigate to={user ? "/quizzes" : "/login"} />}
        />
      </Routes>
    </Router>
  );
}

export default App;
