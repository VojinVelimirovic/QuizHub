import { FiChevronDown, FiChevronUp } from "react-icons/fi";

export default function QuestionCard({
  question,
  questionIndex,
  onQuestionChange,
  onRemoveQuestion,
  onAddAnswerOption,
  onRemoveAnswerOption,
  onAnswerOptionChange,
  onToggleCollapse,
  dragHandleProps
}) {
  return (
    <div ref={dragHandleProps?.innerRef} {...dragHandleProps?.draggableProps} {...dragHandleProps?.dragHandleProps} className="question-card">
      <div className="question-header">Question {questionIndex + 1}</div>
      <button type="button" className="collapse-btn" onClick={() => onToggleCollapse(questionIndex)}>
        {question.collapsed ? <FiChevronDown/> : <FiChevronUp/>}
      </button>

      {!question.collapsed && (
        <>
          <div><br></br><label>Question Text*</label><input value={question.text} onChange={(e) => onQuestionChange(questionIndex, "text", e.target.value)} /></div>
          <div><label>Type</label><select value={question.questionType} onChange={(e) => onQuestionChange(questionIndex, "questionType", e.target.value)}>
            <option value="SingleChoice">Single Choice</option>
            <option value="MultipleChoice">Multiple Choice</option>
            <option value="TrueFalse">True/False</option>
            <option value="FillInTheBlank">Fill In</option>
          </select></div>

          {question.questionType === "FillInTheBlank" ? (
            <div><label>Correct Answer*</label><input type="text" value={question.fillInAnswer} onChange={(e) => onQuestionChange(questionIndex, "fillInAnswer", e.target.value)} placeholder="Correct answer text" /></div>
          ) : (
            <div>
              <h4>Answer Options</h4>
              {question.questionType !== "TrueFalse" && (
                <button type="button" onClick={() => onAddAnswerOption(questionIndex)}>+ Add Option</button>
              )}
              {question.answerOptions.map((ao, aoIndex) => (
                <div key={aoIndex} className="answer-option">
                  <input 
                    value={ao.text} 
                    onChange={(e) => onAnswerOptionChange(questionIndex, aoIndex, "text", e.target.value)} 
                    placeholder="Option text" 
                    disabled={question.questionType === "TrueFalse"}
                  />
                  <label>
                    Correct?
                    <input 
                      type="checkbox" 
                      checked={ao.isCorrect} 
                      onChange={(e) => onAnswerOptionChange(questionIndex, aoIndex, "isCorrect", e.target.checked)} 
                    />
                  </label>
                  {question.questionType !== "TrueFalse" && (
                    <button type="button" onClick={() => onRemoveAnswerOption(questionIndex, aoIndex)}>X</button>
                  )}
                </div>
              ))}
            </div>
          )}

          <button type="button" onClick={() => onRemoveQuestion(questionIndex)} className="remove-btn">Remove Question</button>
        </>
      )}
    </div>
  );
}