export class LiveRoomCreateRequest {
  constructor(name, quizId, maxPlayers, secondsPerQuestion, startDelaySeconds) {
    this.name = name;
    this.quizId = quizId;
    this.maxPlayers = maxPlayers;
    this.secondsPerQuestion = secondsPerQuestion;
    this.startDelaySeconds = startDelaySeconds;
  }
}