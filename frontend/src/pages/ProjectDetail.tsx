import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { projectsAPI, tasksAPI } from '../services/api';
import type { ProjectDetail, Task } from '../types';
import CreateTaskModal from '../components/CreateTaskModal';
import EditTaskModal from '../components/EditTaskModal';
import SchedulerModal from '../components/SchedulerModal';

const ProjectDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [project, setProject] = useState<ProjectDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showCreateTaskModal, setShowCreateTaskModal] = useState(false);
  const [showEditTaskModal, setShowEditTaskModal] = useState(false);
  const [showSchedulerModal, setShowSchedulerModal] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);

  useEffect(() => {
    loadProject();
  }, [id]);

  const loadProject = async () => {
    try {
      setLoading(true);
      const data = await projectsAPI.getById(Number(id));
      setProject(data);
    } catch (err) {
      setError('Failed to load project');
    } finally {
      setLoading(false);
    }
  };

  const handleToggleTask = async (taskId: number) => {
    try {
      await tasksAPI.toggle(taskId);
      loadProject();
    } catch (err) {
      alert('Failed to toggle task');
    }
  };

  const handleDeleteTask = async (taskId: number) => {
    if (!confirm('Are you sure you want to delete this task?')) return;

    try {
      await tasksAPI.delete(taskId);
      loadProject();
    } catch (err) {
      alert('Failed to delete task');
    }
  };

  const handleEditTask = (task: Task) => {
    setSelectedTask(task);
    setShowEditTaskModal(true);
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (error || !project) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 mb-4">{error || 'Project not found'}</p>
          <Link to="/dashboard" className="text-indigo-600 hover:text-indigo-700">
            Back to Dashboard
          </Link>
        </div>
      </div>
    );
  }

  const incompleteTasks = project.tasks.filter(t => !t.isCompleted);
  const completedTasks = project.tasks.filter(t => t.isCompleted);

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Link to="/dashboard" className="text-indigo-600 hover:text-indigo-700 mr-4">
                ← Back
              </Link>
              <h1 className="text-xl font-bold text-gray-900">{project.title}</h1>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {project.description && (
          <p className="text-gray-600 mb-6">{project.description}</p>
        )}

        <div className="flex justify-between items-center mb-6">
          <h2 className="text-xl font-semibold text-gray-900">Tasks</h2>
          <div className="space-x-2">
            <button
              onClick={() => setShowSchedulerModal(true)}
              disabled={incompleteTasks.length === 0}
              className="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
            >
              📅 Smart Schedule
            </button>
            <button
              onClick={() => setShowCreateTaskModal(true)}
              className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700"
            >
              + Add Task
            </button>
          </div>
        </div>

        {project.tasks.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <p className="text-gray-500 text-lg mb-4">No tasks yet</p>
            <button
              onClick={() => setShowCreateTaskModal(true)}
              className="text-indigo-600 hover:text-indigo-700 font-medium"
            >
              Add your first task
            </button>
          </div>
        ) : (
          <div className="space-y-6">
            {incompleteTasks.length > 0 && (
              <div className="bg-white rounded-lg shadow p-6">
                <h3 className="font-semibold text-gray-900 mb-4">Active Tasks</h3>
                <div className="space-y-3">
                  {incompleteTasks.map((task) => (
                    <TaskItem
                      key={task.id}
                      task={task}
                      onToggle={handleToggleTask}
                      onEdit={handleEditTask}
                      onDelete={handleDeleteTask}
                    />
                  ))}
                </div>
              </div>
            )}

            {completedTasks.length > 0 && (
              <div className="bg-white rounded-lg shadow p-6">
                <h3 className="font-semibold text-gray-900 mb-4">Completed Tasks</h3>
                <div className="space-y-3">
                  {completedTasks.map((task) => (
                    <TaskItem
                      key={task.id}
                      task={task}
                      onToggle={handleToggleTask}
                      onEdit={handleEditTask}
                      onDelete={handleDeleteTask}
                    />
                  ))}
                </div>
              </div>
            )}
          </div>
        )}
      </main>

      {showCreateTaskModal && (
        <CreateTaskModal
          projectId={project.id}
          onClose={() => setShowCreateTaskModal(false)}
          onSuccess={() => {
            setShowCreateTaskModal(false);
            loadProject();
          }}
        />
      )}

      {showEditTaskModal && selectedTask && (
        <EditTaskModal
          task={selectedTask}
          onClose={() => {
            setShowEditTaskModal(false);
            setSelectedTask(null);
          }}
          onSuccess={() => {
            setShowEditTaskModal(false);
            setSelectedTask(null);
            loadProject();
          }}
        />
      )}

      {showSchedulerModal && (
        <SchedulerModal
          projectId={project.id}
          onClose={() => setShowSchedulerModal(false)}
        />
      )}
    </div>
  );
};

const TaskItem: React.FC<{
  task: Task;
  onToggle: (id: number) => void;
  onEdit: (task: Task) => void;
  onDelete: (id: number) => void;
}> = ({ task, onToggle, onEdit, onDelete }) => {
  return (
    <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50">
      <div className="flex items-center flex-1">
        <input
          type="checkbox"
          checked={task.isCompleted}
          onChange={() => onToggle(task.id)}
          className="h-5 w-5 text-indigo-600 rounded focus:ring-indigo-500 cursor-pointer"
        />
        <div className="ml-4 flex-1">
          <p className={`${task.isCompleted ? 'line-through text-gray-500' : 'text-gray-900'}`}>
            {task.title}
          </p>
          {task.dueDate && (
            <p className="text-sm text-gray-500 mt-1">
              Due: {new Date(task.dueDate).toLocaleDateString()}
            </p>
          )}
        </div>
      </div>
      <div className="flex space-x-2">
        <button
          onClick={() => onEdit(task)}
          className="text-indigo-600 hover:text-indigo-800 text-sm font-medium"
        >
          Edit
        </button>
        <button
          onClick={() => onDelete(task.id)}
          className="text-red-600 hover:text-red-800 text-sm font-medium"
        >
          Delete
        </button>
      </div>
    </div>
  );
};

export default ProjectDetailPage;