FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine 
WORKDIR /app
COPY ["App/App.csproj", "App/"]
RUN dotnet restore "App/App.csproj"
COPY . .
WORKDIR /app
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
CMD [ "dotnet-ef", "database", "update", "--project", "./App" ]
