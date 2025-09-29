import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_BASE_URL}/results`;

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

    const response = await axios.get(`${API_URL}/my-results`, {
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

    const response = await axios.get(`${API_URL}/leaderboard/${quizId}?top=${top}&timeFilter=${timeFilter}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    return response.data;
  } catch (err) {
    console.error('Error fetching leaderboard:', err);
    throw err;
  }
};

export const getAllResults = async () => {
  const token = localStorage.getItem("token");
  const response = await axios.get(`${API_URL}`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return response.data;
};