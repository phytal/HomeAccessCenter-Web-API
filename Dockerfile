# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY HAC.API/*.csproj ./HAC.API/
RUN dotnet restore

# copy everything else and build app
COPY HAC.API/. ./HAC.API/
WORKDIR /source/HAC.API
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app ./
<<<<<<< HEAD:src/Dockerfile
ENV ASPNETCORE_URLS http://+:5000
EXPOSE 5000
=======
>>>>>>> parent of 8a9cf3f (Docker + Heroku):Dockerfile
ENTRYPOINT ["dotnet", "HAC.API.dll"]