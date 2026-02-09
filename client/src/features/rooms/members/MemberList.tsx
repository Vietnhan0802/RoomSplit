import { Copy, Crown } from 'lucide-react';
import Card from '../../../components/ui/Card';
import Badge from '../../../components/ui/Badge';
import UserAvatar from '../../../components/shared/UserAvatar';
import { showToast } from '../../../components/ui/Toast';
import type { RoomDetail } from '../../../types';

interface MemberListProps {
  room: RoomDetail | null;
}

export default function MemberList({ room }: MemberListProps) {
  if (!room) return null;

  const copyInviteCode = () => {
    navigator.clipboard.writeText(room.inviteCode);
    showToast('success', 'Đã sao chép mã mời!');
  };

  return (
    <div className="space-y-4">
      <Card>
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-gray-500">Mã mời phòng</p>
            <p className="text-2xl font-bold tracking-widest">{room.inviteCode}</p>
          </div>
          <button onClick={copyInviteCode} className="rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700">
            <Copy className="h-5 w-5" />
          </button>
        </div>
      </Card>

      <h2 className="text-lg font-semibold">Thành viên ({room.members.length})</h2>

      <div className="space-y-2">
        {room.members.map((member) => (
          <Card key={member.id}>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <UserAvatar user={member.user} />
                <div>
                  <div className="flex items-center gap-2">
                    <p className="font-medium">{member.user.fullName}</p>
                    {member.role === 'Admin' && <Crown className="h-4 w-4 text-yellow-500" />}
                  </div>
                  <p className="text-xs text-gray-500">{member.user.email}</p>
                </div>
              </div>
              <Badge variant={member.role === 'Admin' ? 'warning' : 'default'}>
                {member.role === 'Admin' ? 'Quản trị' : 'Thành viên'}
              </Badge>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
