# RoomSplit

Ứng dụng quản lý phòng trọ & tài chính cá nhân - Full-stack monorepo.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + TypeScript + Vite + Tailwind CSS 3 + PWA |
| Backend | .NET 8 Web API + C# + Entity Framework Core |
| Database | PostgreSQL 17 |
| File Storage | Local disk (`uploads/`) |

## Cấu trúc dự án

```
RoomSplit/
├── client/          # React frontend
├── server/          # .NET 8 backend
│   ├── RoomSplit.API/            # Controllers, DTOs, Middlewares
│   ├── RoomSplit.Core/           # Entities, Interfaces, Enums
│   └── RoomSplit.Infrastructure/ # DbContext, Repositories, Services
├── uploads/         # File uploads (receipts, avatars)
├── docker-compose.yml
└── README.md
```

## Yêu cầu

- Node.js 18+
- .NET 8 SDK
- PostgreSQL 17 (hoặc Docker)

## Cài đặt & Chạy

### 1. Database

**Option A: Docker (khuyến nghị)**
```bash
docker-compose up -d postgres
```

**Option B: Local PostgreSQL**
- Đảm bảo PostgreSQL đang chạy trên `localhost:5432`
- User: `postgres`, Password: `postgres`

### 2. Backend

```bash
cd server/RoomSplit.API
dotnet run
```

API sẽ chạy tại `http://localhost:5000` (hoặc port mặc định của .NET).

### 3. Frontend

```bash
cd client
npm install
npm run dev
```

Frontend chạy tại `http://localhost:5173`.

## Tính năng chính

- **Quản lý phòng trọ**: Tạo phòng, mời thành viên bằng mã mời
- **Chia sẻ chi phí**: Thêm chi phí chung, chia đều/theo phần trăm/tùy chỉnh
- **Task rotation**: Phân công công việc luân phiên (quét nhà, rửa bát...)
- **Tài chính cá nhân**: Ghi chép thu chi, calendar view
- **Ngân sách**: Đặt hạn mức chi tiêu theo danh mục
- **Báo cáo**: Biểu đồ tổng quan thu chi theo tháng
- **PWA**: Cài đặt như app native, hỗ trợ offline

## API Endpoints

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/auth/register` | Đăng ký |
| POST | `/api/auth/login` | Đăng nhập |
| GET | `/api/auth/me` | Thông tin user hiện tại |
| GET | `/api/rooms` | Danh sách phòng |
| POST | `/api/rooms` | Tạo phòng |
| POST | `/api/rooms/join/{code}` | Tham gia phòng |
| GET | `/api/rooms/{id}/expenses` | Chi phí phòng |
| POST | `/api/rooms/{id}/expenses` | Thêm chi phí |
| GET | `/api/rooms/{id}/tasks` | Công việc phòng |
| POST | `/api/rooms/{id}/tasks` | Tạo công việc |
| GET | `/api/transactions` | Giao dịch cá nhân |
| POST | `/api/transactions` | Thêm giao dịch |
| GET | `/api/budgets` | Ngân sách |
| POST | `/api/budgets` | Tạo ngân sách |
| POST | `/api/files/upload/{folder}` | Upload file |

## File Upload

- Max size: 5MB
- Allowed types: JPG, PNG, WebP
- Auto resize: max 1920px width
- Folders: `uploads/receipts/`, `uploads/avatars/`
