import axios from "axios";

const API_URL = "https://localhost:7208/api/quizzes";

export const getAllQuizzes = async () => {
  const response = await axios.get(API_URL);
  return response.data;
};

export const createFullQuiz = async (quiz, token) => {
  const response = await axios.post(`${API_URL}/full`, quiz, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });
  return response.data;
};

export const getQuizById = async (id) => {
  try {
    const response = await axios.get(`${API_URL}/${id}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching quiz:', error);
    throw error;
  }
};

export const submitQuiz = async (submissionData) => {
  try {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('No authentication token found');
    }

    const response = await axios.post(`${API_URL}/submit`, submissionData, {
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
    });
    return response.data;
  } catch (error) {
    console.error('Error submitting quiz:', error);
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    throw error;
  }
};

export const getUserResults = async () => {
  try {
    const token = localStorage.getItem('token');
    if (!token) throw new Error('No authentication token found');

    const response = await axios.get(`${API_URL}/user-results`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    return response.data;
  } catch (err) {
    console.error('Error fetching user results:', err);
    throw err;
  }
};

export const getQuizLeaderboard = async (quizId, top = 10, timeFilter = "all") => {
  try {
    const token = localStorage.getItem('token');
    if (!token) throw new Error('No authentication token found');

    const response = await axios.get(`${API_URL}/${quizId}/leaderboard?top=${top}&timeFilter=${timeFilter}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    return response.data;
  } catch (err) {
    console.error('Error fetching leaderboard:', err);
    throw err;
  }
};

export const deleteQuiz = async (id, token) => {
  const response = await axios.delete(`${API_URL}/${id}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {}
  });
  return response.data;
};

export const updateFullQuiz = async (id, quiz, token) => {
  const response = await axios.patch(`${API_URL}/${id}/full`, quiz, {
    headers: token ? { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' } : {}
  });
  return response.data;
};
