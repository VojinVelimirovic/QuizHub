import { useState, useEffect, useContext } from "react";
import { getAllCategories, createCategory } from "../services/categoryService";
import { AuthContext } from "../context/AuthContext";

export default function CreateQuizPage() {
  const { token } = useContext(AuthContext);

  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState("");
  const [addingCategory, setAddingCategory] = useState(false);
  const [newCategory, setNewCategory] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    fetchCategories();
  }, []);

  const fetchCategories = async () => {
    try {
      const data = await getAllCategories();
      setCategories(data);
    } catch (err) {
      console.error("Failed to fetch categories", err);
    }
  };

  const handleAddCategory = async () => {
    if (!newCategory.trim()) {
      setError("Category name is required");
      return;
    }

    try {
      const category = await createCategory({ name: newCategory.trim() }, token);
      setCategories([...categories, category]);
      setSelectedCategory(category.id);
      setNewCategory("");
      setError("");
      setAddingCategory(false);
    } catch (err) {
      setError(err.response?.data?.message || "Failed to add category");
    }
  };

  const handleCancel = () => {
    setNewCategory("");
    setError("");
    setAddingCategory(false);
  };

  return (
    <div>
      <h2>Create Quiz</h2>

      <label>Category</label>
      {!addingCategory ? (
        <div style={{ display: "flex", gap: "0.5rem" }}>
          <select
            value={selectedCategory}
            onChange={(e) => setSelectedCategory(e.target.value)}
          >
            <option value="">-- Select Category --</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
          <button type="button" onClick={() => setAddingCategory(true)}>
            +
          </button>
        </div>
      ) : (
        <div style={{ display: "flex", gap: "0.5rem" }}>
          <input
            type="text"
            placeholder="New category name"
            value={newCategory}
            onChange={(e) => setNewCategory(e.target.value)}
          />
          <button type="button" onClick={handleAddCategory}>
            Save
          </button>
          <button type="button" onClick={handleCancel}>
            Cancel
          </button>
        </div>
      )}
      {error && <p style={{ color: "red" }}>{error}</p>}
    </div>
  );
}
