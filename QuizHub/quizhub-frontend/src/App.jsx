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
import ResultsPage from "./pages/ResultsPage";
import CreateRoomPage from "./pages/CreateRoomPage";
import LiveRoomLobby from "./pages/LiveRoomLobby";
import LiveQuizRoom from "./pages/LiveQuizRoom";

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
          element={
            !user ? (
              <Navigate to="/login" />
            ) : user.role === "Admin" ? (
              <UpdateQuizPage />
            ) : (
              <Navigate to="/quizzes" />
            )
          }
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
          path="/results"
          element={
            !user ? (
              <Navigate to="/login" />
            ) : user.role === "Admin" ? (
              <ResultsPage />
            ) : (
              <Navigate to="/quizzes" />
            )
          }
        />
        <Route
          path="/create-room"
          element={
            !user ? (
              <Navigate to="/login" />
            ) : user.role === "Admin" ? (
              <CreateRoomPage />
            ) : (
              <Navigate to="/quizzes" />
            )
          }
        />
        <Route
          path="/live/:roomCode"
          element={user ? <LiveRoomLobby /> : <Navigate to="/login" />}
        />
        <Route
          path="/live/:roomCode/play"
          element={user ? <LiveQuizRoom /> : <Navigate to="/login" />}
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