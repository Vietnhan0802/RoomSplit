FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY RoomSplit.Core/RoomSplit.Core.csproj RoomSplit.Core/
COPY RoomSplit.Infrastructure/RoomSplit.Infrastructure.csproj RoomSplit.Infrastructure/
COPY RoomSplit.API/RoomSplit.API.csproj RoomSplit.API/
RUN dotnet restore RoomSplit.API/RoomSplit.API.csproj

COPY . .
RUN dotnet publish RoomSplit.API/RoomSplit.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RoomSplit.API.dll"]
