import axios from "axios";

const API_URL = "https://localhost:7208/api/users";

export const register = async (userData) => {
  const response = await axios.post(`${API_URL}/register`, userData);
  return response.data;
};

export const login = async (loginData) => {
  const response = await axios.post(`${API_URL}/login`, loginData);
  return response.data;
};
