import { useState, useContext } from "react";
import { login } from "../services/authService";
import { AuthContext } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import { Link } from "react-router-dom";

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
      navigate("/quizzes"); // <-- changed from "/"
    } catch (err) {
      setError(err.response?.data?.message || "Login failed");
    }
  };

  return (
    <div>
      <h2>Login</h2>
      <form onSubmit={handleSubmit}>
        <input name="usernameOrEmail" placeholder="Username or Email" value={form.usernameOrEmail} onChange={handleChange} />
        <input type="password" name="password" placeholder="Password" value={form.password} onChange={handleChange} />
        {error && <p style={{ color: "red" }}>{error}</p>}
        <button type="submit">Login</button>
        <p>
          Don't have an account? <Link to="/register">Register here.</Link>
        </p>
      </form>
    </div>
  );
}
