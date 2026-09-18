FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/DeskFlow.Domain/DeskFlow.Domain.csproj backend/DeskFlow.Domain/
COPY backend/DeskFlow.Application/DeskFlow.Application.csproj backend/DeskFlow.Application/
COPY backend/DeskFlow.Infrastructure/DeskFlow.Infrastructure.csproj backend/DeskFlow.Infrastructure/
COPY backend/DeskFlow.Api/DeskFlow.Api.csproj backend/DeskFlow.Api/
RUN dotnet restore backend/DeskFlow.Api/DeskFlow.Api.csproj

COPY backend/DeskFlow.Domain/ backend/DeskFlow.Domain/
COPY backend/DeskFlow.Application/ backend/DeskFlow.Application/
COPY backend/DeskFlow.Infrastructure/ backend/DeskFlow.Infrastructure/
COPY backend/DeskFlow.Api/ backend/DeskFlow.Api/
RUN dotnet publish backend/DeskFlow.Api/DeskFlow.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DeskFlow.Api.dll"]