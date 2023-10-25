﻿FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine 
RUN addgroup -g 1000 dotnet  && adduser -G dotnet -u 1000 dotnet -D
USER dotnet
WORKDIR /app
COPY --chown=dotnet "App/App.csproj" "App/"
RUN dotnet restore "App/App.csproj"
COPY --chown=dotnet . .
WORKDIR /app
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/home/dotnet/.dotnet/tools"
CMD [ "dotnet-ef", "database", "update", "--project", "./App" ]
