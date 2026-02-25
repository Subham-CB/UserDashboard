FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY *.sln .
COPY UserDashboard.API/*.csproj ./UserDashboard.API/
COPY UserDashboard.Tests/*.csproj ./UserDashboard.Tests/
RUN dotnet restore UserDashboard.sln

COPY UserDashboard.API/ ./UserDashboard.API/
COPY UserDashboard.Tests/ ./UserDashboard.Tests/
RUN dotnet build UserDashboard.sln -c Release --no-restore

FROM build AS test
RUN dotnet test UserDashboard.Tests/UserDashboard.Tests.csproj --no-build -c Release

FROM build AS publish
RUN dotnet publish UserDashboard.API/UserDashboard.API.csproj -c Release -o /publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "UserDashboard.API.dll"]
