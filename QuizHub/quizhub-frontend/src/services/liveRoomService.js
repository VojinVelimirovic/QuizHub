// services/liveRoomService.js
import axios from 'axios';

const API_URL = 'https://localhost:7208/api/liverooms';

export const liveRoomService = {
  // Create a new room (Admin only)
  createRoom: async (roomData, token) => {
    const response = await axios.post(`${API_URL}`, roomData, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  },

  // Get all currently active rooms
  getActiveRooms: async (token) => {
    const response = await axios.get(`${API_URL}/active`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  },

  // Join a room via REST
  // Join a room via REST - IMPROVED ERROR HANDLING
  joinRoom: async (roomCode, token) => {
    try {
      console.log(`Joining room ${roomCode} with token:`, token ? "Token exists" : "No token");
      
      const response = await axios.post(`${API_URL}/${roomCode}/join`, {}, {
        headers: { 
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        timeout: 10000 // 10 second timeout
      });
      
      console.log("Join room response:", response.data);
      return response.data;
    } catch (err) {
      console.error("Join room error details:", {
        message: err.message,
        response: err.response,
        code: err.code,
        config: err.config
      });
      
      // Re-throw with better error message
      if (err.response) {
        // Server responded with error status
        throw new Error(err.response.data.error || err.response.data.message || `Server error: ${err.response.status}`);
      } else if (err.request) {
        // Request was made but no response received
        throw new Error('No response from server. Please check your connection.');
      } else {
        // Something else happened
        throw new Error(err.message || 'Failed to join room');
      }
    }
  },
  startRoom: async (roomCode, token) => {
    try {
      console.log(`Starting room ${roomCode}`);
      
      const response = await axios.post(`${API_URL}/${roomCode}/start`, {}, {
        headers: { 
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        timeout: 10000
      });
      
      console.log("Start room response:", response.data);
      return response.data;
    } catch (err) {
      console.error("Start room error:", err);
      throw new Error(err.response?.data?.error || err.response?.data?.message || 'Failed to start room');
    }
  },
  // Leave a room via REST
  leaveRoom: async (roomCode, token) => {
    const response = await axios.post(`${API_URL}/${roomCode}/leave`, {}, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  },

  // Get lobby status
  getLobbyStatus: async (roomCode, token) => {
    const response = await axios.get(`${API_URL}/${roomCode}/lobby`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  },

  // Forcefully end a room (host only)
  endRoom: async (roomCode, token) => {
    const response = await axios.post(`${API_URL}/${roomCode}/end`, {}, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  }
};
