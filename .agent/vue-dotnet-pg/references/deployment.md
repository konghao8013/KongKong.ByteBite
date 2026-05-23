# Docker 部署与配置

本文件按需加载，不是常驻协议。

## 后端 Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/ByteBite.Api/ByteBite.Api.csproj", "src/ByteBite.Api/"]
COPY ["src/ByteBite.Application/ByteBite.Application.csproj", "src/ByteBite.Application/"]
COPY ["src/ByteBite.Domain/ByteBite.Domain.csproj", "src/ByteBite.Domain/"]
COPY ["src/ByteBite.Infrastructure/ByteBite.Infrastructure.csproj", "src/ByteBite.Infrastructure/"]
COPY ["src/ByteBite.Shared/ByteBite.Shared.csproj", "src/ByteBite.Shared/"]
COPY ["Directory.Build.props", "."]
RUN dotnet restore "src/ByteBite.Api/ByteBite.Api.csproj"
COPY . .
RUN dotnet publish "src/ByteBite.Api/ByteBite.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ByteBite.Api.dll"]
```

## 前端 Dockerfile

```dockerfile
FROM node:22-alpine AS build
WORKDIR /app
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ .
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

## docker-compose.yml

```yaml
services:
  api:
    build:
      context: .
      dockerfile: docker/Dockerfile.api
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Host=192.168.3.22;Port=5432;Database=kongkong_bytebite;Username=konghao;Password=hitek.123
      - Redis__ConnectionString=redis:6379
      - Jwt__Secret=${JWT_SECRET}
      - Jwt__Issuer=ByteBite
      - Jwt__Audience=ByteBite.Web
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_started

  web:
    build:
      context: .
      dockerfile: docker/Dockerfile.web
    ports:
      - "80:80"
    depends_on:
      - api

  postgres:
    image: postgres:17-alpine
    environment:
      POSTGRES_DB: kongkong_bytebite
      POSTGRES_USER: konghao
      POSTGRES_PASSWORD: hitek.123
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U bytebite"]
      interval: 5s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  pgdata:
```

## 环境变量

| 变量 | 说明 | 必需 |
|---|---|---|
| `DB_PASSWORD` | PostgreSQL 密码 | 是 |
| `JWT_SECRET` | JWT 签名密钥 | 是 |
| `VITE_API_BASE_URL` | 前端 API 基础地址 | 是 |
| `ASPNETCORE_ENVIRONMENT` | 运行环境 | 是 |
| `ConnectionStrings__Default` | 数据库连接字符串（Host=192.168.3.22;Port=5432;Database=kongkong_bytebite） | 是 |
| `Redis__ConnectionString` | Redis 连接字符串 | 否 |

## 配置优先级

1. 环境变量（最高）
2. `appsettings.{Environment}.json`
3. `appsettings.json`
4. 默认值（代码中）

## 数据库连接信息

| 项目 | 值 |
|------|---|
| Host | 192.168.3.22 |
| Port | 5432 |
| Database | kongkong_bytebite |
| Username | konghao |
| Password | hitek.123 |

**连接字符串（appsettings.json）：**

```json
{
  "ConnectionStrings": {
    "Default": "Host=192.168.3.22;Port=5432;Database=kongkong_bytebite;Username=konghao;Password=hitek.123"
  }
}
```

**EF Core CLI 迁移命令：**

```bash
dotnet ef migrations add Init --project src/ByteBite.Infrastructure --startup-project src/ByteBite.Api
dotnet ef database update --project src/ByteBite.Infrastructure --startup-project src/ByteBite.Api
```

## 注意事项

- 敏感配置（密码、密钥）禁止写入 `appsettings.json`，使用环境变量或 Secret Manager
- 开发环境使用 `dotnet user-secrets`
- 生产环境使用 Docker 环境变量或 Kubernetes Secrets
- 数据库迁移在 API 启动时自动执行（开发环境），生产环境建议手动执行
