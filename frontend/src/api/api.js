import axios from 'axios';

const API_BASE_URL = 'https://localhost:7087/api/v1';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor to attach token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add response interceptor to handle errors
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authAPI = {
  login: (username, password) =>
    apiClient.post('/auth/login', { username, password }),
  getCurrentUser: () => apiClient.get('/auth/me'),
};

export const employeeAPI = {
  getAll: () => apiClient.get('/employees'),
  getById: (id) => apiClient.get(`/employees/${id}`),
};

export const vacationAPI = {
  getMyBalance: (year) => apiClient.get(`/vacations/balance${year ? `?year=${year}` : ''}`),
  getBalanceHistory: () => apiClient.get('/vacations/balance/history'),
  getMyRequests: () => apiClient.get('/vacations/requests/my'),
  getRequest: (id) => apiClient.get(`/vacations/requests/${id}`),
  getPendingRequests: () => apiClient.get('/vacations/requests/pending'),
  getAllRequests: (filters) => apiClient.get('/vacations/requests', { params: filters }),
  createRequest: (data) => apiClient.post('/vacations/requests', data),
  updateRequest: (id, data) => apiClient.put(`/vacations/requests/${id}`, data),
  approveRequest: (id, comments) => apiClient.post(`/vacations/requests/${id}/approve`, { comments }),
  rejectRequest: (id, reason) => apiClient.post(`/vacations/requests/${id}/reject`, { reason }),
  cancelRequest: (id, reason) => apiClient.post(`/vacations/requests/${id}/cancel`, { reason }),
  getRequestHistory: (id) => apiClient.get(`/vacations/requests/${id}/history`),
};

export const permissionAPI = {
  getTypes: () => apiClient.get('/permissions/types'),
  getMyRequests: () => apiClient.get('/permissions/requests/my'),
  getRequest: (id) => apiClient.get(`/permissions/requests/${id}`),
  getPendingRequests: () => apiClient.get('/permissions/requests/pending'),
  getAllRequests: (filters) => apiClient.get('/permissions/requests', { params: filters }),
  createRequest: (data) => apiClient.post('/permissions/requests', data),
  approveRequest: (id, comments) => apiClient.post(`/permissions/requests/${id}/approve`, { comments }),
  rejectRequest: (id, reason) => apiClient.post(`/permissions/requests/${id}/reject`, { reason }),
  cancelRequest: (id, reason) => apiClient.post(`/permissions/requests/${id}/cancel`, { reason }),
  getMyUsage: (year) => apiClient.get(`/permissions/usage${year ? `?year=${year}` : ''}`),
};

export const notificationAPI = {
  getNotifications: (unreadOnly = false) => apiClient.get(`/notifications?unreadOnly=${unreadOnly}`),
  getUnreadCount: () => apiClient.get('/notifications/unread-count'),
  markAsRead: (id) => apiClient.post(`/notifications/${id}/read`),
  markAllAsRead: () => apiClient.post('/notifications/read-all'),
};

export default apiClient;
