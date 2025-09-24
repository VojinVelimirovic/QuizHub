import axios from "axios";

const API_URL = "https://localhost:7208/api/users";

export const register = async (formData) => {
  const response = await axios.post(`${API_URL}/register`, formData, {
    headers: { "Content-Type": "multipart/form-data" }
  });
  return response.data;
};

export const login = async (loginData) => {
  const response = await axios.post(`${API_URL}/login`, loginData);
  return response.data;
};
