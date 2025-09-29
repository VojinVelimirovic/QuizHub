import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_BASE_URL}/quizzes`;

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