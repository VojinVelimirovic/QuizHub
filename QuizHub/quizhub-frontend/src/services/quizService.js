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
