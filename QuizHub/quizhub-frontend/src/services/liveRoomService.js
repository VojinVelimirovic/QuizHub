import axios from 'axios';

const API_URL = `${import.meta.env.VITE_API_BASE_URL}/liverooms`;

export const liveRoomService = {
  createRoom: async (roomData, token) => {
    const response = await axios.post(`${API_URL}`, roomData, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  },
  getActiveRooms: async (token) => {
    const response = await axios.get(`${API_URL}/active`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  },
  joinRoom: async (roomCode, token) => {
    try {
      console.log(`Joining room ${roomCode} with token:`, token ? "Token exists" : "No token");
      
      const response = await axios.post(`${API_URL}/${roomCode}/join`, {}, {
        headers: { 
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        timeout: 10000
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
      
      if (err.response) {
        throw new Error(err.response.data.error || err.response.data.message || `Server error: ${err.response.status}`);
      } else if (err.request) {
        throw new Error('No response from server. Please check your connection.');
      } else {
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
  leaveRoom: async (roomCode, token) => {
    const response = await axios.post(`${API_URL}/${roomCode}/leave`, {}, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  },
  getLobbyStatus: async (roomCode, token) => {
    const response = await axios.get(`${API_URL}/${roomCode}/lobby`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  },
  endRoom: async (roomCode, token) => {
    const response = await axios.post(`${API_URL}/${roomCode}/end`, {}, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
  }
};
