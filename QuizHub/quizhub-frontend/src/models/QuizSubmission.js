export class QuizSubmission {
  constructor(quizId, answers, durationSeconds) {
    this.quizId = quizId;
    this.answers = answers;
    this.durationSeconds = durationSeconds;
  }
}

export class QuestionAnswer {
  constructor(questionId, selectedAnswerIds = [], textAnswer = null) {
    this.questionId = questionId;
    this.selectedAnswerIds = selectedAnswerIds;
    this.textAnswer = textAnswer;
  }
}