import { useState, useContext } from "react";
import { login } from "../services/authService";
import { AuthContext } from "../context/AuthContext";
import { useNavigate, Link } from "react-router-dom";
import "../styles/main.css";

export default function Login() {
  const { loginUser } = useContext(AuthContext);
  const navigate = useNavigate();
  const [form, setForm] = useState({ usernameOrEmail: "", password: "" });
  const [error, setError] = useState("");

  const handleChange = e => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async e => {
    e.preventDefault();
    setError("");

    if (!form.usernameOrEmail || !form.password) {
      setError("All fields are required");
      return;
    }

    try {
      const { token, user } = await login(form);
      loginUser(user, token);
      navigate("/quizzes");
    } catch (err) {
      setError(err.response?.data?.message || "Login failed");
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Login</h2>
      <input name="usernameOrEmail" placeholder="Username or Email" value={form.usernameOrEmail} onChange={handleChange} />
      <input type="password" name="password" placeholder="Password" value={form.password} onChange={handleChange} />
      {error && <p className="error">{error}</p>}
      <button type="submit">Login</button>
      <p>Don't have an account? <br/><Link to="/register">Register here.</Link></p>
    </form>
  );
}
