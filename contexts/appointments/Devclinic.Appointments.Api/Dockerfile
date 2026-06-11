FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["contexts/appointments/Devclinic.Appointments.Api/Devclinic.Appointments.Api.csproj", "contexts/appointments/Devclinic.Appointments.Api/"]
COPY ["contexts/appointments/Devclinic.Appointments.Infrastructure/Devclinic.Appointments.Infrastructure.csproj", "contexts/appointments/Devclinic.Appointments.Infrastructure/"]
COPY ["contexts/appointments/Devclinic.Appointments.Application/Devclinic.Appointments.Application.csproj", "contexts/appointments/Devclinic.Appointments.Application/"]
COPY ["contexts/appointments/Devclinic.Appointments.Domain/Devclinic.Appointments.Domain.csproj", "contexts/appointments/Devclinic.Appointments.Domain/"]
RUN dotnet restore "contexts/appointments/Devclinic.Appointments.Api/Devclinic.Appointments.Api.csproj"
COPY . .
WORKDIR "/src/contexts/appointments/Devclinic.Appointments.Api"
RUN dotnet build "./Devclinic.Appointments.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Devclinic.Appointments.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Devclinic.Appointments.Api.dll"]
