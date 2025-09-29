import * as signalR from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
    this.roomCode = null;
    this.onPlayerJoined = null;
    this.onPlayerLeft = null;
    this.onRoomJoined = null;
    this.onRoomLeft = null;
    this.onQuizStarted = null;
    this.onQuestionStarted = null;
    this.onQuestionEnded = null;
    this.onAnswerSubmitted = null;
    this.onLeaderboardUpdated = null;
    this.onQuizEnded = null;
    this.onError = null;
    this.onLobbyStatus = null;
  }

  initializeConnection = () => {
    const token = localStorage.getItem('token');

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(import.meta.env.VITE_SIGNALR_URL, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .build();

    this.setupEventHandlers();
    return this.connection;
  };

  setupEventHandlers = () => {
    if (!this.connection) return;

    this.connection.on('PlayerJoined', (userId) => {
      console.log('PlayerJoined event received:', userId);
      this.onPlayerJoined?.({ userId });
    });
    
    this.connection.on('PlayerLeft', (userId) => {
      console.log('PlayerLeft event received:', userId);
      this.onPlayerLeft?.({ userId });
    });
    
    this.connection.on('RoomJoined', (roomCode) => {
      console.log('RoomJoined event received:', roomCode);
      this.onRoomJoined?.(roomCode);
    });
    
    this.connection.on('RoomLeft', (roomCode) => {
      console.log('RoomLeft event received:', roomCode);
      this.onRoomLeft?.(roomCode);
    });
    
    this.connection.on('QuizStarted', () => {
      console.log('QuizStarted event received');
      this.onQuizStarted?.();
    });
    
    this.connection.on('LobbyStatus', (lobby) => {
      console.log('LobbyStatus event received:', lobby);
      this.onLobbyStatus?.(lobby);
    });

    this.connection.on('QuestionStarted', (question) => this.onQuestionStarted?.(question));
    this.connection.on('QuestionEnded', (questionId) => this.onQuestionEnded?.(questionId));
    this.connection.on('AnswerSubmitted', (questionId) => this.onAnswerSubmitted?.(questionId));
    this.connection.on('LeaderboardUpdated', (leaderboard) => this.onLeaderboardUpdated?.(leaderboard));
    this.connection.on('QuizEnded', (finalLeaderboard) => this.onQuizEnded?.(finalLeaderboard));
    
    this.connection.onclose((error) => {
      console.log('SignalR connection closed:', error);
      this.isConnected = false;
      this.onError?.(new Error(`Connection closed: ${error}`));
    });
    
    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting:', error);
      this.onError?.(new Error(`Reconnecting: ${error}`));
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      this.isConnected = true;
      if (this.roomCode) {
        this.joinRoom(this.roomCode);
      }
    });
  };

  startConnection = async () => {
    if (!this.connection) throw new Error('Connection not initialized');
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      this.isConnected = true;
      return;
    }

    try {
      await this.connection.start();
      this.isConnected = true;
      console.log('SignalR Connected');
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      this.isConnected = false;
      throw err;
    }
  };

  stopConnection = async () => {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (err) {
        console.error('Error stopping connection:', err);
      }
      this.connection = null;
      this.roomCode = null;
      this.isConnected = false;
    }
  };

  joinRoom = async (roomCode) => {
    if (!this.connection || !this.isConnected) {
      throw new Error('No connection available');
    }
    this.roomCode = roomCode;
    await this.connection.invoke('JoinRoom', roomCode);
  };

  leaveRoom = async () => {
    if (!this.connection || !this.roomCode || !this.isConnected) {
      return;
    }
    try {
      await this.connection.invoke('LeaveRoom', this.roomCode);
    } catch (err) {
      console.error('Error leaving room:', err);
    }
    this.roomCode = null;
  };

  startQuiz = async () => {
    if (!this.connection || !this.roomCode || !this.isConnected) {
      throw new Error('No room joined or connection lost');
    }
    await this.connection.invoke('StartQuiz', this.roomCode);
  };

  submitAnswer = async (questionId, answer) => {
    if (!this.connection || !this.roomCode) return;
    
    const submission = {
      questionId: questionId,
      answer: answer,
      clientSubmittedAt: Date.now()
    };
    
    console.log("🟡 Submitting answer:", submission);
    await this.connection.invoke('SubmitAnswer', submission);
  };

  ensureConnectedAndJoin = async (roomCode) => {
    if (!this.connection) {
      this.initializeConnection();
    }

    if (!this.isConnected) {
      await this.startConnection();
    }

    if (this.roomCode && this.roomCode !== roomCode) {
      try { await this.leaveRoom(); } catch (e) { }
    }

    if (this.roomCode !== roomCode) {
      await this.joinRoom(roomCode);
    }
  };

  advanceQuestion = async () => {
    if (!this.connection || !this.roomCode) throw new Error('No room joined');
    await this.connection.invoke('AdvanceQuestion');
  };

  setOnError(callback) { 
    this.onError = callback; 
  }
  setOnLobbyStatus(callback) { 
    this.onLobbyStatus = callback; 
  }
setOnReconnecting(callback) { 
  this.connection?.onreconnecting(() => {
    console.log('SignalR reconnecting...');
    callback?.();
  });
}

setOnReconnected(callback) { 
  this.connection?.onreconnected(() => {
    console.log('SignalR reconnected');
    callback?.();
  });
}
  setOnPlayerJoined(callback) { this.onPlayerJoined = callback; }
  setOnPlayerLeft(callback) { this.onPlayerLeft = callback; }
  setOnRoomJoined(callback) { this.onRoomJoined = callback; }
  setOnRoomLeft(callback) { this.onRoomLeft = callback; }
  setOnQuizStarted(callback) { this.onQuizStarted = callback; }
  setOnQuestionStarted(callback) { this.onQuestionStarted = callback; }
  setOnQuestionEnded(callback) { this.onQuestionEnded = callback; }
  setOnAnswerSubmitted(callback) { this.onAnswerSubmitted = callback; }
  setOnLeaderboardUpdated(callback) { this.onLeaderboardUpdated = callback; }
  setOnQuizEnded(callback) { this.onQuizEnded = callback; }
}

export const signalRService = new SignalRService();
