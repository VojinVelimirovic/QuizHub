import { useState, useContext } from "react";
import { register, login } from "../services/authService";
import { AuthContext } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import { Link } from "react-router-dom";

export default function Register() {
  const { loginUser } = useContext(AuthContext);
  const navigate = useNavigate();
  
  const [form, setForm] = useState({ username: "", email: "", password: "" });
  const [error, setError] = useState("");

  const handleChange = e => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async e => {
    e.preventDefault();
    setError("");

    if (!form.username || !form.email || !form.password) {
      setError("All fields are required");
      return;
    }

    try {
      await register(form);
      const { token, user } = await login({ usernameOrEmail: form.username, password: form.password });
      loginUser(user, token);

      navigate("/quizzes"); // <-- changed from "/"
    } catch (err) {
      setError(err.response?.data?.message || "Registration failed");
    }
  };

  return (
    <div>
      <h2>Register</h2>
      <form onSubmit={handleSubmit}>
        <input name="username" placeholder="Username" value={form.username} onChange={handleChange} />
        <input name="email" placeholder="Email" value={form.email} onChange={handleChange} />
        <input type="password" name="password" placeholder="Password" value={form.password} onChange={handleChange} />
        {error && <p style={{ color: "red" }}>{error}</p>}
        <button type="submit">Register</button>
        <p>
          Already have an account? <Link to="/login">Login here.</Link>
        </p>
      </form>
    </div>
  );
}
