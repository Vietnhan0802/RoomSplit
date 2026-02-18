import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Plus, CheckCircle, Clock, RotateCw } from 'lucide-react';
import Button from '../../../components/ui/Button';
import Card from '../../../components/ui/Card';
import Badge from '../../../components/ui/Badge';
import Modal from '../../../components/ui/Modal';
import TaskForm from '../../../components/forms/TaskForm';
import UserAvatar from '../../../components/shared/UserAvatar';
import { roomsApi } from '../../../api/rooms';
import { showToast } from '../../../components/ui/showToast';
import { formatDate } from '../../../utils/dateHelpers';
import type { RoomTask } from '../../../types';

export default function TaskList() {
  const { roomId } = useParams<{ roomId: string }>();
  const [tasks, setTasks] = useState<RoomTask[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (roomId) {
      roomsApi.getTasks(roomId).then((res) => setTasks(res.data.data || [])).finally(() => setIsLoading(false));
    }
  }, [roomId]);

  const handleCreate = async (data: { title: string; description?: string; frequency: number; isRotating: boolean }) => {
    if (!roomId) return;
    try {
      setIsSubmitting(true);
      const res = await roomsApi.createTask(roomId, data);
      if (res.data.data) {
        setTasks((prev) => [...prev, res.data.data!]);
        setShowForm(false);
        showToast('success', 'Tạo công việc thành công!');
      }
    } catch {
      showToast('error', 'Tạo công việc thất bại');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleComplete = async (taskId: string) => {
    if (!roomId) return;
    try {
      await roomsApi.completeTask(roomId, taskId);
      showToast('success', 'Hoàn thành!');
    } catch {
      showToast('error', 'Không thể hoàn thành');
    }
  };

  const statusVariant = (status: string) => {
    switch (status) {
      case 'Completed': return 'success';
      case 'InProgress': return 'warning';
      case 'Skipped': return 'danger';
      default: return 'default';
    }
  };

  if (isLoading) return <p className="text-center text-gray-400 py-8">Đang tải...</p>;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">Công việc</h2>
        <Button size="sm" onClick={() => setShowForm(true)}>
          <Plus className="mr-1.5 h-4 w-4" /> Tạo mới
        </Button>
      </div>

      {tasks.length === 0 ? (
        <p className="py-8 text-center text-gray-400">Chưa có công việc nào</p>
      ) : (
        <div className="space-y-3">
          {tasks.map((task) => (
            <Card key={task.id}>
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="font-medium">{task.title}</h3>
                    {task.isRotating && <RotateCw className="h-3.5 w-3.5 text-gray-400" />}
                  </div>
                  {task.description && <p className="text-sm text-gray-500">{task.description}</p>}
                  <Badge className="mt-1">{task.frequency}</Badge>
                </div>
              </div>

              {task.assignments.length > 0 && (
                <div className="mt-3 space-y-2">
                  {task.assignments.map((a) => (
                    <div key={a.id} className="flex items-center justify-between rounded-lg bg-gray-50 p-2 dark:bg-gray-700/50">
                      <div className="flex items-center gap-2">
                        <UserAvatar user={a.assignedTo} size="sm" />
                        <div>
                          <p className="text-sm">{a.assignedTo.fullName}</p>
                          <p className="text-xs text-gray-400"><Clock className="inline h-3 w-3" /> {formatDate(a.dueDate)}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        <Badge variant={statusVariant(a.status)}>{a.status}</Badge>
                        {a.status === 'Pending' && (
                          <button onClick={() => handleComplete(task.id)} className="text-green-600 hover:text-green-700">
                            <CheckCircle className="h-5 w-5" />
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </Card>
          ))}
        </div>
      )}

      <Modal isOpen={showForm} onClose={() => setShowForm(false)} title="Tạo công việc mới">
        <TaskForm onSubmit={handleCreate} isLoading={isSubmitting} />
      </Modal>
    </div>
  );
}
