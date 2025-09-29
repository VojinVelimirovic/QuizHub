import { useState, useEffect, useContext } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getQuizById, updateFullQuiz } from "../services/quizService";
import { getAllCategories } from "../services/categoryService";
import { AuthContext } from "../context/AuthContext";
import Navbar from "../components/Navbar";
import { DragDropContext, Droppable, Draggable } from "@hello-pangea/dnd";
import "../styles/CreateQuizPage.css";
import QuestionCard from "../components/QuizForm/QuestionCard";

export default function UpdateQuizPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { token } = useContext(AuthContext);

  const [categories, setCategories] = useState([]);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [timeLimit, setTimeLimit] = useState("");
  const [difficulty, setDifficulty] = useState("Easy");
  const [questions, setQuestions] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState("");
  const [error, setError] = useState("");
  const [deletedAnswerIds, setDeletedAnswerIds] = useState([]);

  useEffect(() => {
    const load = async () => {
      try {
        const [quiz, cats] = await Promise.all([getQuizById(id), getAllCategories()]);
        setCategories(cats);
        setTitle(quiz.title || "");
        setDescription(quiz.description || "");
        setTimeLimit(String(quiz.timeLimitMinutes || ""));
        setDifficulty(quiz.difficulty || "Easy");
        setSelectedCategory(String(quiz.categoryId || ""));

        const mapped = (quiz.questions || []).map(q => {
          let answerOptions = [];
          let fillInAnswer = "";

          if (q.questionType === "FillInTheBlank") {
            fillInAnswer = q.textAnswer || "";
          } else if (q.questionType === "TrueFalse") {
            answerOptions = [
              { 
                id: q.answerOptions.find(ao => ao.text === "True")?.id ?? null, 
                text: "True", 
                isCorrect: q.answerOptions.some(ao => ao.text === "True" && ao.isCorrect) 
              },
              { 
                id: q.answerOptions.find(ao => ao.text === "False")?.id ?? null, 
                text: "False", 
                isCorrect: q.answerOptions.some(ao => ao.text === "False" && ao.isCorrect) 
              }
            ];
          } else {
            answerOptions = (q.answerOptions || []).map(ao => ({
              id: ao.id,
              text: ao.text || "",
              isCorrect: ao.isCorrect || false
            }));
          }

          return {
            id: q.id,
            text: q.text || "",
            questionType: q.questionType,
            fillInAnswer,
            answerOptions,
            collapsed: false
          };
        });

        setQuestions(mapped);
      } catch (err) {
        console.error("Failed to load quiz or categories:", err);
        setError("Failed to load quiz data");
      }
    };

    load();
  }, [id]);

  const handleQuestionChange = (qIndex, field, value) => {
    const updated = [...questions];
    updated[qIndex][field] = value;

    if (field === "questionType") {
      if (value === "TrueFalse") {
        updated[qIndex].answerOptions = [
          { text: "True", isCorrect: false },
          { text: "False", isCorrect: false },
        ];
        updated[qIndex].fillInAnswer = "";
      } else if (value === "FillInTheBlank") {
        updated[qIndex].answerOptions = [];
      }
    }

    setQuestions(updated);
  };

  const handleAddQuestion = () => {
    setQuestions(prev => [
      ...prev,
      {
        text: "",
        questionType: "SingleChoice",
        answerOptions: [],
        fillInAnswer: "",
        collapsed: false,
      },
    ]);
  };

  const handleRemoveQuestion = qIndex =>
    setQuestions(prev => prev.filter((_, i) => i !== qIndex));

  const handleAddAnswerOption = qIndex => {
    const updated = [...questions];
    updated[qIndex].answerOptions.push({
      text: "",
      isCorrect: false,
      tempId: Date.now() + Math.random()
    });
    setQuestions(updated);
  };

  const handleRemoveAnswerOption = (qIndex, aoIndex) => {
    const updated = [...questions];
    const removed = updated[qIndex].answerOptions.splice(aoIndex, 1)[0];

    if (removed.id && typeof removed.id === 'number') {
      setDeletedAnswerIds(prev => [...prev, removed.id]);
    }

    setQuestions(updated);
  };

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

  const toggleCollapse = qIndex => {
    const updated = [...questions];
    updated[qIndex].collapsed = !updated[qIndex].collapsed;
    setQuestions(updated);
  };

  const onDragEnd = result => {
    if (!result.destination) return;
    const reordered = Array.from(questions);
    const [moved] = reordered.splice(result.source.index, 1);
    reordered.splice(result.destination.index, 0, moved);
    setQuestions(reordered);
  };

  const handleSubmit = async e => {
    e.preventDefault();
    setError("");
    
    if (!title.trim()) return setError("Title is required");
    if (!selectedCategory) return setError("Category is required");
    if (!timeLimit || isNaN(timeLimit) || Number(timeLimit) <= 0) return setError("Time limit must be a positive number");
    if (questions.length === 0) return setError("At least one question is required");

    for (let i = 0; i < questions.length; i++) {
      const q = questions[i];
      if (!q.text.trim()) return setError(`Question ${i + 1} must have text`);
      
      if (q.questionType === "FillInTheBlank") {
        if (!q.fillInAnswer.trim()) return setError(`Question ${i + 1}: Fill-in questions must have a correct answer`);
      } else {
        if (q.answerOptions.length === 0) return setError(`Question ${i + 1} must have at least one answer option`);
        if (!q.answerOptions.some(ao => ao.isCorrect)) return setError(`Question ${i + 1} must have at least one correct answer`);
        if (q.answerOptions.some(ao => !ao.text.trim())) return setError(`Question ${i + 1}: All answer options must have text`);
      }
    }

    const quizPayload = {
      title,
      description,
      categoryId: Number(selectedCategory),
      timeLimitMinutes: Number(timeLimit),
      difficulty,
      deletedAnswerIds,
      questions: questions.map(q => ({
        id: q.id ?? null,
        text: q.text,
        questionType: q.questionType,
        answerOptions: q.questionType === "FillInTheBlank" ? [] : q.answerOptions.map(ao => ({
          id: ao.id ?? null,
          text: ao.text,
          isCorrect: ao.isCorrect
        })),
        textAnswer: q.questionType === "FillInTheBlank" ? q.fillInAnswer : null
      })),
    };

    try {
      await updateFullQuiz(id, quizPayload, token || localStorage.getItem("token"));
      alert("Quiz updated successfully!");
      navigate("/quizzes");
    } catch (err) {
      console.error("Update error:", err);
      
      const errorMessage = err.response?.data?.message || err.message || "Failed to update quiz";
      
      if (errorMessage.includes("quiz results") || errorMessage.includes("been selected")) {
        setError(
          "❌ Cannot modify this quiz because people have already taken it and selected these answers. " +
          "You can create a new version of the quiz instead, or only add new questions/answers without removing existing ones."
        );
      } else {
        setError(errorMessage);
      }
    }
  };

  return (
    <div className="create-quiz-page">
      <Navbar />
      <div className="form-scroll-container">
        <h2>Update Quiz</h2>
        <form onSubmit={handleSubmit}>
          <div>
            <label>Title*</label>
            <input value={title} onChange={e => setTitle(e.target.value)} />
          </div>
          <div>
            <label>Description</label>
            <textarea value={description} onChange={e => setDescription(e.target.value)} />
          </div>
          <div>
            <label>Category*</label>
            <select value={selectedCategory} onChange={e => setSelectedCategory(e.target.value)}>
              <option value="">-- Select Category --</option>
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <div>
            <label>Time Limit (minutes)*</label>
            <input type="number" value={timeLimit} onChange={e => setTimeLimit(e.target.value)} />
          </div>
          <div>
            <label>Difficulty</label>
            <select value={difficulty} onChange={e => setDifficulty(e.target.value)}>
              <option>Easy</option>
              <option>Medium</option>
              <option>Hard</option>
            </select>
          </div>
          <div className="questions-header">
            <h3>Questions</h3>
            <button type="button" onClick={handleAddQuestion}>+ Add Question</button>
          </div>
          <DragDropContext onDragEnd={onDragEnd}>
            <Droppable droppableId="questions">
              {provided => (
                <div {...provided.droppableProps} ref={provided.innerRef}>
                  {questions.map((q, qIndex) => (
                    <Draggable key={q.id ?? qIndex} draggableId={`q-${qIndex}`} index={qIndex}>
                      {(provided) => (
                        <QuestionCard
                          question={q}
                          questionIndex={qIndex}
                          dragHandleProps={provided}
                          onQuestionChange={handleQuestionChange}
                          onRemoveQuestion={handleRemoveQuestion}
                          onAddAnswerOption={handleAddAnswerOption}
                          onRemoveAnswerOption={handleRemoveAnswerOption}
                          onAnswerOptionChange={handleAnswerOptionChange}
                          onToggleCollapse={toggleCollapse}
                        />
                      )}
                    </Draggable>
                  ))}
                  {provided.placeholder}
                </div>
              )}
            </Droppable>
          </DragDropContext>
          {error && <p className="error">{error}</p>}
          <button type="submit">Update Quiz</button>
        </form>
      </div>
    </div>
  );
}