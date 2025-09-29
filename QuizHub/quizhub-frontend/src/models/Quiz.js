export class Quiz {
  constructor(title, description, categoryId, timeLimitMinutes, difficulty, questions) {
    this.title = title;
    this.description = description;
    this.categoryId = categoryId;
    this.timeLimitMinutes = timeLimitMinutes;
    this.difficulty = difficulty;
    this.questions = questions;
  }

  static createUpdatePayload(title, description, categoryId, timeLimitMinutes, difficulty, questions, deletedAnswerIds = []) {
    const payload = new Quiz(title, description, categoryId, timeLimitMinutes, difficulty, questions);
    payload.deletedAnswerIds = deletedAnswerIds;
    return payload;
  }

  static createFromFormData(formData, questions) {
    return new Quiz(
      formData.title,
      formData.description,
      formData.categoryId,
      Number(formData.timeLimitMinutes),
      formData.difficulty,
      questions.map(q => ({
        text: q.text,
        questionType: q.questionType,
        answerOptions: q.questionType === "FillInTheBlank" 
          ? [{ text: q.fillInAnswer, isCorrect: true }] 
          : q.answerOptions,
        textAnswer: q.questionType === "FillInTheBlank" ? q.fillInAnswer : null
      }))
    );
  }
}