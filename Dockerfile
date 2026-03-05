# ========================================
# Stage 1: Build
# ========================================
# Build (Sử dụng SDK nặng để biên dịch code)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj và restore các thư viện (NuGet) trước
# Việc này giúp Docker tận dụng cache, nếu bạn không đổi thư viện thì lần build sau sẽ cực nhanh
COPY docShare_Backend.slnx .
COPY Domain/Domain.csproj Domain/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY docShare/API.csproj docShare/

# Restore dependencies
RUN dotnet restore docShare/API.csproj

# Copy toàn bộ code còn lại vào thư mục /src
COPY . .

# Build và Publish project ra thư mục /app/publish
RUN dotnet publish docShare/API.csproj -c Release -o /app/publish --no-restore

# ========================================
# Stage 2: Runtime
# ========================================
# Giai đoạn 2: Runtime (Sử dụng môi trường chạy siêu nhẹ)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Tạo user không có quyền root (tăng bảo mật)
RUN adduser --disabled-password --gecos "" appuser

# Copy file đã build từ Stage 1 sang
COPY --from=build /app/publish .

# Expose cổng mà API sẽ chạy (mặc định .NET 8 trong Docker dùng cổng 8080)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# Switch to non-root user
USER appuser


ENTRYPOINT ["dotnet", "API.dll"]
