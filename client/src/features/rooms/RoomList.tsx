import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, UserPlus, Users } from 'lucide-react';
import Card from '../../components/ui/Card';
import Button from '../../components/ui/Button';
import Modal from '../../components/ui/Modal';
import Input from '../../components/ui/Input';
import RoomForm from '../../components/forms/RoomForm';
import { CardSkeleton } from '../../components/ui/Skeleton';
import { roomsApi } from '../../api/rooms';
import { showToast } from '../../components/ui/showToast';
import type { Room } from '../../types';

export default function RoomList() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [showJoin, setShowJoin] = useState(false);
  const [inviteCode, setInviteCode] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    roomsApi.getMyRooms().then((res) => setRooms(res.data.data || [])).finally(() => setIsLoading(false));
  }, []);

  const handleCreate = async (data: { name: string; description?: string }) => {
    try {
      setIsSubmitting(true);
      const res = await roomsApi.createRoom(data);
      if (res.data.data) {
        setRooms((prev) => [...prev, res.data.data!]);
        setShowCreate(false);
        showToast('success', 'Tạo phòng thành công!');
      }
    } catch {
      showToast('error', 'Tạo phòng thất bại');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleJoin = async () => {
    try {
      setIsSubmitting(true);
      const res = await roomsApi.joinRoom(inviteCode);
      if (res.data.data) {
        setRooms((prev) => [...prev, res.data.data!]);
        setShowJoin(false);
        setInviteCode('');
        showToast('success', 'Tham gia phòng thành công!');
      }
    } catch {
      showToast('error', 'Mã mời không hợp lệ');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map((i) => <CardSkeleton key={i} />)}
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Phòng trọ</h1>
        <div className="flex gap-2">
          <Button variant="secondary" size="sm" onClick={() => setShowJoin(true)}>
            <UserPlus className="mr-1.5 h-4 w-4" /> Tham gia
          </Button>
          <Button size="sm" onClick={() => setShowCreate(true)}>
            <Plus className="mr-1.5 h-4 w-4" /> Tạo phòng
          </Button>
        </div>
      </div>

      {rooms.length === 0 ? (
        <Card className="py-12 text-center">
          <Users className="mx-auto mb-3 h-12 w-12 text-gray-300" />
          <p className="text-gray-500">Bạn chưa có phòng nào</p>
          <p className="text-sm text-gray-400">Tạo phòng mới hoặc nhập mã mời để tham gia</p>
        </Card>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {rooms.map((room) => (
            <Link key={room.id} to={`/rooms/${room.id}`}>
              <Card className="hover:shadow-md transition-shadow">
                <div className="flex items-center gap-3">
                  <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary-100 text-xl font-bold text-primary-700">
                    {room.name.charAt(0)}
                  </div>
                  <div>
                    <h3 className="font-semibold">{room.name}</h3>
                    {room.description && <p className="text-sm text-gray-500">{room.description}</p>}
                    <p className="text-xs text-gray-400">{room.memberCount} thành viên</p>
                  </div>
                </div>
              </Card>
            </Link>
          ))}
        </div>
      )}

      <Modal isOpen={showCreate} onClose={() => setShowCreate(false)} title="Tạo phòng mới">
        <RoomForm onSubmit={handleCreate} isLoading={isSubmitting} />
      </Modal>

      <Modal isOpen={showJoin} onClose={() => setShowJoin(false)} title="Tham gia phòng">
        <div className="space-y-4">
          <Input label="Mã mời" value={inviteCode} onChange={(e) => setInviteCode(e.target.value)} placeholder="Nhập mã mời 6 ký tự" />
          <Button className="w-full" onClick={handleJoin} isLoading={isSubmitting} disabled={!inviteCode}>
            Tham gia
          </Button>
        </div>
      </Modal>
    </div>
  );
}
