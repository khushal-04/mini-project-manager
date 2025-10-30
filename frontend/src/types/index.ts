export interface User {
  email: string;
  name: string;
  token: string;
}

export interface LoginData {
  email: string;
  password: string;
}

export interface RegisterData {
  email: string;
  password: string;
  name: string;
}

export interface Project {
  id: number;
  title: string;
  description?: string;
  createdAt: string;
  taskCount: number;
  completedTaskCount: number;
}

export interface ProjectDetail {
  id: number;
  title: string;
  description?: string;
  createdAt: string;
  tasks: Task[];
}

export interface Task {
  id: number;
  title: string;
  dueDate?: string;
  isCompleted: boolean;
  createdAt: string;
  projectId: number;
}

export interface CreateProjectData {
  title: string;
  description?: string;
}

export interface UpdateProjectData {
  title: string;
  description?: string;
}

export interface CreateTaskData {
  title: string;
  dueDate?: string;
}

export interface UpdateTaskData {
  title: string;
  dueDate?: string;
  isCompleted: boolean;
}

export interface ScheduleRequest {
  availableHoursPerDay: number;
  workingDays: number[];
  startDate: string;
}

export interface ScheduledTask {
  taskId: number;
  title: string;
  suggestedStartDate: string;
  suggestedDueDate: string;
  estimatedHours: number;
  priority: string;
}

export interface ScheduleResponse {
  scheduledTasks: ScheduledTask[];
  totalEstimatedHours: number;
  message: string;
}