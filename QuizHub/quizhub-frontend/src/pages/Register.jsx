import { useState, useContext } from "react";
import { register, login } from "../services/authService";
import { AuthContext } from "../context/AuthContext";
import { useNavigate, Link } from "react-router-dom";
import "../styles/main.css";

export default function Register() {
  const { loginUser } = useContext(AuthContext);
  const navigate = useNavigate();
  const [form, setForm] = useState({ username: "", email: "", password: "" });
  const [profileFile, setProfileFile] = useState(null);
  const [error, setError] = useState("");

  const handleChange = e => setForm({ ...form, [e.target.name]: e.target.value });
  const handleFileChange = e => setProfileFile(e.target.files[0]);

  const handleSubmit = async e => {
    e.preventDefault();
    setError("");

    if (!form.username || !form.email || !form.password || !profileFile) {
      setError("All fields including profile picture are required");
      return;
    }

    try {
      const formData = new FormData();
      formData.append("Username", form.username);
      formData.append("Email", form.email);
      formData.append("Password", form.password);
      formData.append("profileImage", profileFile);

      await register(formData);

      // auto-login
      const { token, user } = await login({ usernameOrEmail: form.username, password: form.password });
      loginUser(user, token);
      navigate("/quizzes");
    } catch (err) {
      setError(err.response?.data?.message || "Registration failed");
    }
  };

  return (
    <form onSubmit={handleSubmit} encType="multipart/form-data">
      <h2>Register</h2>
      <input name="username" placeholder="Username" value={form.username} onChange={handleChange} />
      <input name="email" placeholder="Email" value={form.email} onChange={handleChange} />
      <input type="password" name="password" placeholder="Password" value={form.password} onChange={handleChange} />
      <div>
        <label>Profile Picture*</label>
        <input type="file" accept="image/*" onChange={handleFileChange} />
      </div>
      {error && <p className="error">{error}</p>}
      <button type="submit">Register</button>
      <p>Already have an account? <Link to="/login">Login here.</Link></p>
    </form>
  );
}
