# Build and Test
Start docker containers executing:
```powershell
docker compose up -d
```
```powershell
docker compose up -d --build --force-recreate
```

If the PostgreSQL container is new, apply the migrations:
```
dotnet ef database update
```

- Main application is available on http://localhost:8081/ wich is mapped to 5020 on the container.
- Seq container dashboard is available on http://localhost:8082/ and uses 5341 for log registration.
- Jaeger container dashboard is available on http://localhost:8083/ and uses 4317 for trace registration.
