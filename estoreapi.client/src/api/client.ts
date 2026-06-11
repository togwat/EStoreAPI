import axios from "axios";

export const api = axios.create({
  withCredentials: true,   // send the session cookie
});

// authentication checker
// every response passes through here
api.interceptors.response.use(
  // success: pass through untouched
  (response) => response,
  // login required
  (error) => {
    if (error.response?.status === 401 && window.location.pathname !== "/login") {
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);
