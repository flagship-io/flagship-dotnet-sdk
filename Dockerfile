FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY Flagship/* ./Flagship/
COPY Flagship.QAApp/*.csproj ./Flagship.QAApp/
WORKDIR /source/Flagship.QAApp
RUN dotnet restore

# copy everything else and build app
COPY Flagship.QAApp/. ./
RUN dotnet publish -c Release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Flagship.QAApp.dll"]