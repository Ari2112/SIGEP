import axios from 'axios';

const API_BASE_URL = 'http://localhost:5017/api/v1';

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
  create: (data) => apiClient.post('/employees', data),
  update: (id, data) => apiClient.put(`/employees/${id}`, data),
  deactivate: (id) => apiClient.delete(`/employees/${id}`),
  getPositions: () => apiClient.get('/employees/positions'),
  createPosition: (data) => apiClient.post('/employees/positions', data),
  getSchedules: () => apiClient.get('/employees/schedules'),
  createSchedule: (data) => apiClient.post('/employees/schedules', data),
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
  getPendingApproval: () => apiClient.get('/permissions/requests/pending'),
  getAllRequests: (filters) => apiClient.get('/permissions/requests', { params: filters }),
  create: (data) => apiClient.post('/permissions/requests', data),
  approve: (id, data) => apiClient.post(`/permissions/requests/${id}/approve`, data),
  reject: (id, data) => apiClient.post(`/permissions/requests/${id}/reject`, data),
  cancel: (id, reason) => apiClient.post(`/permissions/requests/${id}/cancel`, { reason }),
  getUsageSummary: (year) => apiClient.get(`/permissions/usage${year ? `?year=${year}` : ''}`),
};

export const attendanceAPI = {
  getToday: () => apiClient.get('/attendance/today'),
  getMyRecords: (dateFrom, dateTo) =>
    apiClient.get('/attendance/my', { params: { dateFrom, dateTo } }),
  getByEmployee: (employeeId, dateFrom, dateTo) =>
    apiClient.get(`/attendance/employee/${employeeId}`, { params: { dateFrom, dateTo } }),
  getAll: (filters) => apiClient.get('/attendance', { params: filters }),
  checkIn: (notes) => apiClient.post('/attendance/check-in', { notes }),
  checkOut: (data) => apiClient.post('/attendance/check-out', data),
};

export const overtimeAPI = {
  getAll: (filters) => apiClient.get('/overtime', { params: filters }),
  getMy: (filters) => apiClient.get('/overtime/my', { params: filters }),
  getByEmployee: (employeeId, filters) =>
    apiClient.get(`/overtime/employee/${employeeId}`, { params: filters }),
  getById: (id) => apiClient.get(`/overtime/${id}`),
  review: (id, approve, comments) =>
    apiClient.post(`/overtime/${id}/review`, { approve, comments }),
  justify: (id, justification) =>
    apiClient.post(`/overtime/${id}/justify`, { justification }),
};

export const payrollAPI = {
  getAll: () => apiClient.get('/payroll'),
  getById: (id) => apiClient.get(`/payroll/${id}`),
  generate: (data) => apiClient.post('/payroll/generate', data),
  approve: (id, notes) => apiClient.post(`/payroll/${id}/approve`, { notes }),
  annul: (id, notes) => apiClient.post(`/payroll/${id}/annul`, { notes }),
  getDeductionTypes: () => apiClient.get('/payroll/deduction-types'),
  getBenefitTypes: () => apiClient.get('/payroll/benefit-types'),
};

export const settlementAPI = {
  getAll: () => apiClient.get('/settlement'),
  getById: (id) => apiClient.get(`/settlement/${id}`),
  getByEmployee: (empId) => apiClient.get(`/settlement/employee/${empId}`),
  calculate: (data) => apiClient.post('/settlement/calculate', data),
  approve: (id, notes) => apiClient.post(`/settlement/${id}/approve`, { notes }),
  markAsPaid: (id) => apiClient.post(`/settlement/${id}/pay`),
};

export const annualBonusAPI = {
  getAll: () => apiClient.get('/annualbonus'),
  getById: (id) => apiClient.get(`/annualbonus/${id}`),
  getByYear: (year) => apiClient.get(`/annualbonus/year/${year}`),
  calculate: (data) => apiClient.post('/annualbonus/calculate', data),
  approve: (id, notes) => apiClient.post(`/annualbonus/${id}/approve`, { notes }),
  recalculate: (id) => apiClient.post(`/annualbonus/${id}/recalculate`),
};

export const evaluationAPI = {
  getAll: (filters) => apiClient.get('/performanceevaluation', { params: filters }),
  getMy: () => apiClient.get('/performanceevaluation/my'),
  getByEmployee: (empId) => apiClient.get(`/performanceevaluation/employee/${empId}`),
  getById: (id) => apiClient.get(`/performanceevaluation/${id}`),
  create: (data) => apiClient.post('/performanceevaluation', data),
  update: (id, data) => apiClient.put(`/performanceevaluation/${id}`, data),
  acknowledge: (id) => apiClient.post(`/performanceevaluation/${id}/acknowledge`),
};

export const disabilityAPI = {
  getAll: (filters) => apiClient.get('/disability', { params: filters }),
  getMy: () => apiClient.get('/disability/my'),
  getByEmployee: (empId) => apiClient.get(`/disability/employee/${empId}`),
  getById: (id) => apiClient.get(`/disability/${id}`),
  create: (data) => apiClient.post('/disability', data),
  createForEmployee: (empId, data) => apiClient.post(`/disability/employee/${empId}`, data),
  review: (id, approve, comments) => apiClient.post(`/disability/${id}/review`, { approve, comments }),
};

export const reportAPI = {
  getDashboardStats: () => apiClient.get('/report/dashboard'),
  getAttendanceReport: (filters) => apiClient.get('/report/attendance', { params: filters }),
  getOvertimeReport: (filters) => apiClient.get('/report/overtime', { params: filters }),
  getPayrollReport: (payrollId) => apiClient.get(`/report/payroll/${payrollId}`),
};

export const notificationAPI = {
  getNotifications: (unreadOnly = false) => apiClient.get(`/notifications?unreadOnly=${unreadOnly}`),
  getUnreadCount: () => apiClient.get('/notifications/unread-count'),
  markAsRead: (id) => apiClient.post(`/notifications/${id}/read`),
  markAllAsRead: () => apiClient.post('/notifications/read-all'),
};

export default apiClient;