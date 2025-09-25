import { useState, useEffect, useContext } from "react";
import { DragDropContext, Droppable, Draggable } from "@hello-pangea/dnd";
import { getAllCategories, createCategory } from "../services/categoryService";
import { createFullQuiz } from "../services/quizService";
import { AuthContext } from "../context/AuthContext";
import Navbar from "../components/Navbar";
import { FiChevronDown, FiChevronUp } from "react-icons/fi";
import "../styles/CreateQuizPage.css";

export default function CreateQuizPage() {
  const { token } = useContext(AuthContext);

  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState("");
  const [addingCategory, setAddingCategory] = useState(false);
  const [newCategory, setNewCategory] = useState("");
  const [error, setError] = useState("");

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [timeLimit, setTimeLimit] = useState("");
  const [difficulty, setDifficulty] = useState("Easy");
  const [questions, setQuestions] = useState([]);

  useEffect(() => { fetchCategories(); }, []);

  const fetchCategories = async () => {
    try { const data = await getAllCategories(); setCategories(data); }
    catch (err) { console.error("Failed to fetch categories", err); }
  };

  const handleAddCategory = async () => {
    if (!newCategory.trim()) { setError("Category name is required"); return; }
    try {
      const category = await createCategory({ name: newCategory.trim() }, token);
      setCategories([...categories, category]);
      setSelectedCategory(category.id);
      setNewCategory("");
      setError("");
      setAddingCategory(false);
    } catch (err) { setError(err.response?.data?.message || "Failed to add category"); }
  };

  const handleCancelCategory = () => { setNewCategory(""); setError(""); setAddingCategory(false); };

  const handleAddQuestion = () => {
    setQuestions([...questions, { text: "", questionType: "SingleChoice", answerOptions: [], fillInAnswer: "", collapsed: false }]);
  };

  const handleRemoveQuestion = (qIndex) => { setQuestions(questions.filter((_, i) => i !== qIndex)); };

  const handleQuestionChange = (qIndex, field, value) => {
    const updated = [...questions]; 
    const previousType = updated[qIndex].questionType;
    updated[qIndex][field] = value;
    
    if (field === "questionType") {
      if (value === "SingleChoice") {
        if (previousType === "TrueFalse") {
          updated[qIndex].answerOptions = [];
        } else {
          updated[qIndex].answerOptions = updated[qIndex].answerOptions.map(ao => ({ 
            ...ao, 
            isCorrect: false 
          }));
        }
      } 
      else if (value === "MultipleChoice") {
        if (previousType === "TrueFalse") {
          updated[qIndex].answerOptions = [];
        }
      }
      else if (value === "TrueFalse") {
        updated[qIndex].answerOptions = [
          { text: "True", isCorrect: false },
          { text: "False", isCorrect: false }
        ];
      }
      else if (value === "FillInTheBlank") {
        updated[qIndex].answerOptions = [];
      }
    }
    setQuestions(updated);
  };

  const handleAddAnswerOption = (qIndex) => {
    const updated = [...questions]; updated[qIndex].answerOptions.push({ text: "", isCorrect: false }); setQuestions(updated);
  };

  const handleRemoveAnswerOption = (qIndex, aoIndex) => { const updated = [...questions]; updated[qIndex].answerOptions.splice(aoIndex, 1); setQuestions(updated); };

  const handleAnswerOptionChange = (qIndex, aoIndex, field, value) => {
    const updated = [...questions];
    
    if (field === "isCorrect") {
      if (questions[qIndex].questionType === "SingleChoice" || questions[qIndex].questionType === "TrueFalse") {
        updated[qIndex].answerOptions = updated[qIndex].answerOptions.map((ao, i) => ({ 
          ...ao, 
          isCorrect: i === aoIndex ? value : false 
        }));
      } else {
        updated[qIndex].answerOptions[aoIndex][field] = value;
      }
    } else {
      updated[qIndex].answerOptions[aoIndex][field] = value;
    }
    setQuestions(updated);
  };

  const toggleCollapse = (qIndex) => { const updated = [...questions]; updated[qIndex].collapsed = !updated[qIndex].collapsed; setQuestions(updated); };

  const onDragEnd = (result) => {
    if (!result.destination) return;
    const reordered = Array.from(questions); const [moved] = reordered.splice(result.source.index, 1);
    reordered.splice(result.destination.index, 0, moved); setQuestions(reordered);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!title.trim()) return setError("Title is required");
    if (!selectedCategory) return setError("Category is required");
    if (!timeLimit || isNaN(timeLimit) || Number(timeLimit) <= 0) return setError("Time limit must be a positive number");
    if (questions.length === 0) return setError("At least one question is required");

    for (let q of questions) {
      if (!q.text.trim()) return setError("Each question must have text");
      if (q.questionType === "FillInTheBlank") { if (!q.fillInAnswer.trim()) return setError("FillIn question must have a correct text answer"); }
      else { if (!q.answerOptions.length || !q.answerOptions.some(ao => ao.isCorrect)) return setError("Each question must have at least one correct answer"); }
    }

    const quiz = {
      title, description, categoryId: selectedCategory, timeLimitMinutes: Number(timeLimit), difficulty,
      questions: questions.map(q => ({
        text: q.text,
        questionType: q.questionType,
        answerOptions: q.questionType === "FillInTheBlank" ? [{ text: q.fillInAnswer, isCorrect: true }] : q.answerOptions,
        textAnswer: q.questionType === "FillInTheBlank" ? q.fillInAnswer : null
      }))
    };

    try {await createFullQuiz(quiz, token); alert("Quiz created successfully!"); setTitle(""); setDescription(""); setSelectedCategory(""); setTimeLimit(""); setDifficulty("Easy"); setQuestions([]); setError(""); }
    catch (err) { setError(err.response?.data?.message || "Failed to create quiz"); }
  };

  return (
    <div className="create-quiz-page">
      <Navbar />
      <div className="form-scroll-container">
      <h2>Create Quiz</h2>
        <form onSubmit={handleSubmit}>
          <div><label>Title*</label><input value={title} onChange={(e) => setTitle(e.target.value)} /></div>
          <div><label>Description</label><textarea value={description} onChange={(e) => setDescription(e.target.value)} /></div>

          <div>
            <label>Category*</label>
            {!addingCategory ? (
              <div className="category-section">
                <select value={selectedCategory} onChange={(e) => setSelectedCategory(e.target.value)}>
                  <option value="">-- Select Category --</option>
                  {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
                <button type="button" onClick={() => setAddingCategory(true)}>+</button>
              </div>
            ) : (
              <div className="add-category-section">
                <input type="text" placeholder="New category name" value={newCategory} onChange={(e) => setNewCategory(e.target.value)} />
                <button type="button" onClick={handleAddCategory}>Save</button>
                <button type="button" onClick={handleCancelCategory}>Cancel</button>
              </div>
            )}
          </div>

          <div><label>Time Limit (minutes)*</label><input type="number" value={timeLimit} onChange={(e) => setTimeLimit(e.target.value)} /></div>
          <div><label>Difficulty</label><select value={difficulty} onChange={(e) => setDifficulty(e.target.value)}><option>Easy</option><option>Medium</option><option>Hard</option></select></div>

          <div className="questions-header">
            <h3>Questions</h3>
            <button type="button" onClick={handleAddQuestion}>+ Add Question</button>
          </div>

          <DragDropContext onDragEnd={onDragEnd}>
            <Droppable droppableId="questions">
              {(provided) => (
                <div {...provided.droppableProps} ref={provided.innerRef}>
                  {questions.map((q, qIndex) => (
                    <Draggable key={qIndex} draggableId={`q-${qIndex}`} index={qIndex}>
                      {(provided) => (
                        <div ref={provided.innerRef} {...provided.draggableProps} {...provided.dragHandleProps} className="question-card">
                          <div className="question-header">Question {qIndex + 1}</div>
                          <button type="button" className="collapse-btn" onClick={() => toggleCollapse(qIndex)}>
                            {q.collapsed ? <FiChevronDown/> : <FiChevronUp/>}
                          </button>

                          {!q.collapsed && (
                            <>
                              <div><br></br><label>Question Text*</label><input value={q.text} onChange={(e) => handleQuestionChange(qIndex, "text", e.target.value)} /></div>
                              <div><label>Type</label><select value={q.questionType} onChange={(e) => handleQuestionChange(qIndex, "questionType", e.target.value)}>
                                <option value="SingleChoice">Single Choice</option>
                                <option value="MultipleChoice">Multiple Choice</option>
                                <option value="TrueFalse">True/False</option>
                                <option value="FillInTheBlank">Fill In</option>
                              </select></div>

                              {q.questionType === "FillInTheBlank" ? (
                                <div><label>Correct Answer*</label><input type="text" value={q.fillInAnswer} onChange={(e) => handleQuestionChange(qIndex, "fillInAnswer", e.target.value)} placeholder="Correct answer text" /></div>
                              ) : (
                                <div>
                                  <h4>Answer Options</h4>
                                  {q.questionType !== "TrueFalse" && (
                                    <button type="button" onClick={() => handleAddAnswerOption(qIndex)}>+ Add Option</button>
                                  )}
                                  {q.answerOptions.map((ao, aoIndex) => (
                                    <div key={aoIndex} className="answer-option">
                                      <input 
                                        value={ao.text} 
                                        onChange={(e) => handleAnswerOptionChange(qIndex, aoIndex, "text", e.target.value)} 
                                        placeholder="Option text" 
                                        disabled={q.questionType === "TrueFalse"}
                                      />
                                      <label>
                                        Correct?
                                        <input 
                                          type="checkbox" 
                                          checked={ao.isCorrect} 
                                          onChange={(e) => handleAnswerOptionChange(qIndex, aoIndex, "isCorrect", e.target.checked)} 
                                        />
                                      </label>
                                      {q.questionType !== "TrueFalse" && (
                                        <button type="button" onClick={() => handleRemoveAnswerOption(qIndex, aoIndex)}>X</button>
                                      )}
                                    </div>
                                  ))}
                                </div>
                              )}

                              <button type="button" onClick={() => handleRemoveQuestion(qIndex)} className="remove-btn">Remove Question</button>
                            </>
                          )}
                        </div>
                      )}
                    </Draggable>
                  ))}
                  {provided.placeholder}
                </div>
              )}
            </Droppable>
          </DragDropContext>

          {error && <p className="error">{error}</p>}
          <button type="submit">Create Quiz</button>
        </form>
      </div>
    </div>
  );
}
