# DocShare Backend

Dự án Backend cho hệ thống DocShare, được xây dựng trên nền tảng .NET 8 với kiến trúc theo chuẩn Clean Architecture kết hợp với FastEndpoints, đảm bảo hiệu năng cao, linh hoạt và dễ bảo trì.

## 🛠 Tổng Quan Về Công Nghệ Sử Dụng

*   **Framework**: .NET 8.0 (ASP.NET Core Web API)
*   **Kiến trúc định tuyến (Routing)**: [FastEndpoints](https://fastendpoints.com/) thay thế cho Controllers tĩnh truyền thống.
*   **Cơ sở dữ liệu (Database)**: SQL Server
*   **ORM**: Entity Framework Core 8 (Database First)
*   **Lưu trữ Object (Storage)**: MinIO (Tương thích chuẩn AWS S3) dùng cho lưu file PDF và Avatar.
*   **Hàng đợi thông điệp (Message Queue)**: RabbitMQ để xử lý các tác vụ nền tảng bất đồng bộ.
*   **Xác thực và phân quyền (Authentication/Authorization)**: JWT Bearer Token & Google Authentication.
*   **Rate Limiting**: Custom policies để chống spam requests.
*   **Thư viện đọc xuất PDF**: PdfPig

---

## � Tổng Quan Kiến Trúc File

Dự án được chia folder tuân thủ nghiêm ngặt theo kiến trúc nhiều tầng (Clean Architecture):

```text
d:\docShare_Backend/
├── Application/        # Lớp Ứng dụng: Chứa Core Business Logic, Interfaces, CQRS Handlers (CQRS Pattern).
├── Domain/             # Lớp Domain: Chứa các Entity models đại diện cho các bảng trong Database.
├── Infrastructure/     # Lớp Hạ tầng: Chứa DbContext của EF Core, Classes thực thi Repositories, và file Migrations.
└── docShare/           # Lớp API (Presentation): Dự án khởi chạy khởi đầu (Program.cs), định nghĩa API Endpoints, và chứa các file cấu hình appsettings.json.
```

---

## 🚀 Hướng Dẫn Cài Đặt (Từ Bước Clone)

### 1. Clone repository về máy
Mở Terminal/Command Prompt và chạy các lệnh sau:
```bash
git clone https://github.com/DucThangDuong/Backend_DocShare.git
cd docShare_Backend
```

### 2. Yêu cầu hệ thống thiết yếu
*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   **Docker** & **Docker Compose** (để khởi chạy nhanh các external services mà không cần cài đặt trực tiếp lên máy thực tế).

### 3. Cài đặt Docker để chạy Services (RabbitMQ, MinIO)
Tạo file có tên `docker-compose.yml` tại thư mục gốc của dự án `d:\docShare_Backend` với nội dung sau:

```yaml
version: '3.8'
services:
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"

  minio:
    image: minio/minio
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: "admin"
      MINIO_ROOT_PASSWORD: "password123"
    ports:
      - "9000:9000"
      - "9001:9001"
```

Sau đó mở terminal tại chính thư mục này và chạy:
```bash
docker-compose up -d
```
> **Giải thích:** Lệnh này sẽ kéo Image và chạy RabbitMQ ở port `15672`, và khu vực quản trị MinIO ở `9001` trên nền background.

### 4. Cấu hình file `appsettings.json`
Đảm bảo bạn cập nhật lại file `docShare/appsettings.json` hoặc tạo file `docShare/appsettings.Development.json` giống như sau để kết nối với các Docker Services:

```json
{
  "ConnectionStrings": {
    "DocShare": "Server=localhost;Database=DocShare;User Id=sa;Password=YourStrong!Password123;TrustServerCertificate=True"
  },
  "SecretKey": "YourStrongSecretKey",
  "Storage": {
    "ServiceUrl": "http://localhost:9000",
    "AccessKey": "admin",
    "SecretKey": "password123",
    "File_storage": "pdf-storage",
    "Avatar_storage": "avatar-storage"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest",
    "Port": 5672
  },
  "Authentication": {
    "Google": {
      "ClientId": "YourClientID",
      "ClientSecret": "YourClientSecret"
    }
  }
}
```

**Bắt buộc:** Bạn cần truy cập MinIO (`http://localhost:9001`), đăng nhập bằng admin/password123 rồi tạo trước 2 bucket: `pdf-storage` và `avatar-storage`.

### 5. Khởi tạo Database (Apply Migrations)
Sử dụng công cụ command line của EF Core để đẩy thiết kế bảng vào SQL Server Docker:
```bash
cd docShare
dotnet tool install --global dotnet-ef  # <- Chạy để cài giả sử máy bạn chưa có dotnet-ef
dotnet ef database update --project ../Infrastructure/Infrastructure.csproj --startup-project API.csproj
```

### 6. Chạy dự án Backend
Tiếp tục đứng tại thư mục `docShare` trên terminal:
```bash
dotnet run
```
API đã sẵn sàng hoạt động!

---

## 🗄 Tổng Quan Database

Database **DocShare** được thiết kế thông qua Entity Framework Core (Code First). Trọng tâm bao gồm các thành phần:
- **Tài khoản (Users/Roles):** Phục vụ đăng nhập, quản lý cấp quyền hội viên và sinh viên.
- **Tài liệu (Documents):** Lưu trữ siêu dữ liệu (metadata) của file, đường dẫn tới MinIO Storage Object thay vì lưu trực tiếp vào db.
- **Phân loại (Categories/Tags/Universities):** Các bảng cấu trúc mapping giúp tìm kiếm, filter theo trường đại học.
- **Tương tác (Interactions):** Lưu trữ theo vết lượt thích (Like), lượt tải xuống (Download) và Nhận xét (Comments).

**Ví dụ một hình tham khảo Cấu trúc Database Schema thực tế (Minh hoạ ERD):**

![Database Schema Example](https://drive.google.com/file/d/12aXbMn3Q-kEu4psFsiyZljW2CyoARbo1/view?usp=drive_link) 

*(Lưu ý: Đây là ảnh minh hoạ ERD concept để dễ hình dung quan hệ các bảng Entity, cấu trúc thực sẽ dựa trên DbContext có trong codebase).*

---

## 🌐 Link Tới Repository Frontend

Để hệ thống được kết hợp hoàn chỉnh với giao diện Web UI, hãy clone và setup độc lập dự án Frontend bên dưới:

👉 **[Truy cập DocShare Frontend Git Repository tại đây](https://github.com/DucThangDuong/WebDocShare)**

Sau khi mở lên, hãy chỉnh sửa `VITE_API_URL` trong file `.env` Frontend trỏ về thẳng link `https://localhost:PORT` (Backend đang khởi chạy ở trên) để sử dụng toàn vẹn hệ thống.
