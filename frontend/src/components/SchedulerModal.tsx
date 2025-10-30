import React, { useState } from 'react';
import { tasksAPI } from '../services/api';
import type { ScheduleResponse } from '../types';

interface Props {
  projectId: number;
  onClose: () => void;
}

const SchedulerModal: React.FC<Props> = ({ projectId, onClose }) => {
  const [availableHours, setAvailableHours] = useState(8);
  const [workingDays, setWorkingDays] = useState([1, 2, 3, 4, 5]); // Mon-Fri
  const [startDate, setStartDate] = useState(new Date().toISOString().split('T')[0]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [schedule, setSchedule] = useState<ScheduleResponse | null>(null);

  const daysOfWeek = [
    { value: 0, label: 'Sun' },
    { value: 1, label: 'Mon' },
    { value: 2, label: 'Tue' },
    { value: 3, label: 'Wed' },
    { value: 4, label: 'Thu' },
    { value: 5, label: 'Fri' },
    { value: 6, label: 'Sat' }
  ];

  const toggleDay = (day: number) => {
    if (workingDays.includes(day)) {
      setWorkingDays(workingDays.filter(d => d !== day));
    } else {
      setWorkingDays([...workingDays, day].sort());
    }
  };

  const handleGenerate = async () => {
    setError('');

    if (workingDays.length === 0) {
      setError('Please select at least one working day');
      return;
    }

    if (availableHours < 1 || availableHours > 24) {
      setError('Available hours must be between 1 and 24');
      return;
    }

    setLoading(true);

    try {
      const result = await tasksAPI.schedule(projectId, {
        availableHoursPerDay: availableHours,
        workingDays,
        startDate
      });
      setSchedule(result);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to generate schedule');
    } finally {
      setLoading(false);
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'high': return 'text-red-600 bg-red-100';
      case 'medium': return 'text-yellow-600 bg-yellow-100';
      case 'low': return 'text-green-600 bg-green-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50 overflow-y-auto">
      <div className="bg-white rounded-lg max-w-3xl w-full p-6 my-8">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">Smart Task Scheduler</h2>
        
        {error && (
          <div className="mb-4 rounded-md bg-red-50 p-4">
            <p className="text-sm text-red-800">{error}</p>
          </div>
        )}

        {!schedule ? (
          <div>
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Available Hours Per Day
              </label>
              <input
                type="number"
                min="1"
                max="24"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                value={availableHours}
                onChange={(e) => setAvailableHours(Number(e.target.value))}
              />
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Working Days
              </label>
              <div className="flex gap-2">
                {daysOfWeek.map(day => (
                  <button
                    key={day.value}
                    type="button"
                    onClick={() => toggleDay(day.value)}
                    className={`px-4 py-2 rounded-md text-sm font-medium ${
                      workingDays.includes(day.value)
                        ? 'bg-indigo-600 text-white'
                        : 'bg-gray-200 text-gray-700'
                    }`}
                  >
                    {day.label}
                  </button>
                ))}
              </div>
            </div>

            <div className="mb-6">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Start Date
              </label>
              <input
                type="date"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
              />
            </div>

            <div className="flex justify-end space-x-3">
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleGenerate}
                disabled={loading}
                className="px-4 py-2 text-sm font-medium text-white bg-green-600 rounded-md hover:bg-green-700 disabled:bg-green-400"
              >
                {loading ? 'Generating...' : 'Generate Schedule'}
              </button>
            </div>
          </div>
        ) : (
          <div>
            <div className="mb-4 p-4 bg-green-50 rounded-md">
              <p className="text-sm text-green-800">{schedule.message}</p>
              <p className="text-sm text-green-700 mt-2">
                Total estimated hours: <strong>{schedule.totalEstimatedHours}</strong>
              </p>
            </div>

            <div className="space-y-3 max-h-96 overflow-y-auto mb-4">
              {schedule.scheduledTasks.map((task, index) => (
                <div key={task.taskId} className="border border-gray-200 rounded-lg p-4">
                  <div className="flex justify-between items-start mb-2">
                    <h3 className="font-semibold text-gray-900">{index + 1}. {task.title}</h3>
                    <span className={`px-2 py-1 rounded text-xs font-medium ${getPriorityColor(task.priority)}`}>
                      {task.priority}
                    </span>
                  </div>
                  <div className="grid grid-cols-2 gap-2 text-sm text-gray-600">
                    <div>
                      <span className="font-medium">Start:</span> {new Date(task.suggestedStartDate).toLocaleDateString()}
                    </div>
                    <div>
                      <span className="font-medium">Due:</span> {new Date(task.suggestedDueDate).toLocaleDateString()}
                    </div>
                    <div className="col-span-2">
                      <span className="font-medium">Estimated:</span> {task.estimatedHours} hours
                    </div>
                  </div>
                </div>
              ))}
            </div>

            <div className="flex justify-end">
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700"
              >
                Close
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default SchedulerModal;