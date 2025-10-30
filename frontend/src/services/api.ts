import axios from 'axios';
import type {
  LoginData,
  RegisterData,
  User,
  Project,
  ProjectDetail,
  CreateProjectData,
  UpdateProjectData,
  CreateTaskData,
  UpdateTaskData,
  Task,
  ScheduleRequest,
  ScheduleResponse
} from '../types';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5004/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auth API
export const authAPI = {
  register: async (data: RegisterData): Promise<User> => {
    const response = await api.post('/auth/register', data);
    return response.data;
  },

  login: async (data: LoginData): Promise<User> => {
    const response = await api.post('/auth/login', data);
    return response.data;
  },
};

// Projects API
export const projectsAPI = {
  getAll: async (): Promise<Project[]> => {
    const response = await api.get('/projects');
    return response.data;
  },

  getById: async (id: number): Promise<ProjectDetail> => {
    const response = await api.get(`/projects/${id}`);
    return response.data;
  },

  create: async (data: CreateProjectData): Promise<Project> => {
    const response = await api.post('/projects', data);
    return response.data;
  },

  update: async (id: number, data: UpdateProjectData): Promise<Project> => {
    const response = await api.put(`/projects/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/projects/${id}`);
  },
};

// Tasks API
export const tasksAPI = {
  getByProject: async (projectId: number): Promise<Task[]> => {
    const response = await api.get(`/projects/${projectId}/tasks`);
    return response.data;
  },

  create: async (projectId: number, data: CreateTaskData): Promise<Task> => {
    const response = await api.post(`/projects/${projectId}/tasks`, data);
    return response.data;
  },

  update: async (id: number, data: UpdateTaskData): Promise<Task> => {
    const response = await api.put(`/tasks/${id}`, data);
    return response.data;
  },

  toggle: async (id: number): Promise<Task> => {
    const response = await api.patch(`/tasks/${id}/toggle`);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/tasks/${id}`);
  },

  schedule: async (projectId: number, data: ScheduleRequest): Promise<ScheduleResponse> => {
    const response = await api.post(`/projects/${projectId}/schedule`, data);
    return response.data;
  },
};

export default api;