# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file first to maximize layer caching for restore
COPY AgentService.csproj ./
RUN dotnet restore "AgentService.csproj"

# Copy the remaining source and publish
COPY . ./
RUN dotnet publish "AgentService.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

# The app currently uses C:/Test/... paths. In Linux containers these are
# relative paths under WORKDIR, so create them here and seed an empty log file.
RUN mkdir -p "C:/Test/Logs" "C:/Test/Reports" \
    && touch "C:/Test/Logs/logs2.log"

COPY --from=build /app/publish ./

# Default configuration. Override these at docker run time as needed.
ENV DOTNET_ENVIRONMENT=Production \
    Provider=OpenAI

ENTRYPOINT ["dotnet", "AgentService.dll"]
