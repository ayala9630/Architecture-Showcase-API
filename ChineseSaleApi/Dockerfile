# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["ChineseSaleApi/ChineseSaleApi.csproj", "ChineseSaleApi/"]
RUN dotnet restore "ChineseSaleApi/ChineseSaleApi.csproj"

# Copy the entire project
COPY . .

# Build the application
WORKDIR "/src/ChineseSaleApi"
RUN dotnet build "ChineseSaleApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ChineseSaleApi.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Expose the port (change 5000 if your API uses a different port)
EXPOSE 5000
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "ChineseSaleApi.dll"]
