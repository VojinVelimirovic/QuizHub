import axios from "axios";

const API_URL = "https://localhost:7208/api/categories";

export const getAllCategories = async () => {
  const response = await axios.get(API_URL);
  return response.data;
};

export const createCategory = async (category, token) => {
  const response = await axios.post(API_URL, category, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return response.data;
};
