import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Quizzes from "./pages/Quizzes";

function App() {
  const isAuthenticated = !!localStorage.getItem("token");

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route
          path="/quizzes"
          element={isAuthenticated ? <Quizzes /> : <Navigate to="/login" />}
        />
        <Route path="*" element={<Navigate to={isAuthenticated ? "/quizzes" : "/login"} />} />
      </Routes>
    </Router>
  );
}

export default App;
