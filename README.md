# DocShare Backend

Dự án Backend cho hệ thống DocShare, được xây dựng trên nền tảng .NET 8, áp dụng kiến trúc Clean Architecture để đảm bảo khả năng mở rộng và bảo trì.

## 🛠 Công Nghệ Sử Dụng

Dự án sử dụng các công nghệ và thư viện hiện đại sau:

*   **Framework**: .NET 8.0 (ASP.NET Core Web API)
*   **Cơ sở dữ liệu (Database)**: SQL Server
*   **ORM**: Entity Framework Core (Code First)
*   **Lưu trữ (Storage)**: AWS S3 (hoặc MinIO/S3 compatible services)
*   **Giao tiếp thời gian thực (Real-time)**: SignalR
*   **Hàng đợi thông điệp (Message Queue)**: RabbitMQ
*   **Xác thực (Authentication)**: JWT Bearer, Google Authentication
*   **Rate Limiting**: Custom fixed window & IP-based policies

## 📂 Cấu Trúc Dự Án

*   **API**: Chứa các Controllers, cấu hình Program.cs, DI container.
*   **Application**: Chứa Business Logic, Interfaces, DTOs.
*   **Infrastructure**: Triển khai các Interfaces (Repositories, Services), DbContext, Migrations.
*   **Domain**: Chứa các Entity models (chưa thấy rõ trong danh sách file nhưng thường là vậy).

## 🚀 Cài Đặt & Chạy Dự Án

### 1. Yêu cầu hệ thống
*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   SQL Server
*   RabbitMQ Server (có thể chạy qua Docker)
*   Công cụ quản lý S3 (AWS hoặc MinIO)

### 2. Cấu hình
Dự án yêu cầu file cấu hình `appsettings.json`. Do vấn đề bảo mật, file này **không được đẩy lên Git**. Bạn cần tạo file `appsettings.json` trong thư mục `docShare/` (nơi chứa file `API.csproj`) với nội dung mẫu sau:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DocShare": "Server=YOUR_SERVER;Database=DocShareDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "SecretKey": "YOUR_SUPER_SECRET_KEY_FOR_JWT_TOKEN",
  "Storage": {
    "AccessKey": "YOUR_S3_ACCESS_KEY",
    "SecretKey": "YOUR_S3_SECRET_KEY",
    "ServiceUrl": "https://s3.amazonaws.com" 
  }
}
```
*Lưu ý: Thay thế các giá trị `YOUR_...` bằng thông tin cấu hình thực tế của bạn.*

### 3. Cài đặt Database
Chạy lệnh sau để áp dụng Migrations vào SQL Server:

```bash
cd docShare
dotnet ef database update --project ../Infrastructure/Infrastructure.csproj --startup-project API.csproj
```
*(Hoặc dùng Visual Studio Package Manager Console)*

### 4. Chạy ứng dụng
Tại thư mục gốc của dự án:

```bash
dotnet run --project docShare/API.csproj
```
API sẽ khởi chạy (mặc định tại `http://localhost:5204` hoặc `https://localhost:7251` tùy cấu hình launchSettings).

## 🛡 Git & Bảo Mật

File `.gitignore` đã được cấu hình để **bỏ qua** các file nhạy cảm và file rác hệ thống, bao gồm:
*   `appsettings.json`, `appsettings.Development.json` (Chứa key và connection string).
*   Thư mục `bin/`, `obj/` (File build).
*   `.vs/`, `.idea/` (Cấu hình IDE).

**Lưu ý quan trọng**: Tuyệt đối không xóa các dòng ignore `appsettings.json` trong `.gitignore` để tránh lộ khóa bảo mật (Secret Key, Database credentials).
