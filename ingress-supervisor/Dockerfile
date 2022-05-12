FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

RUN mkdir app
COPY *.csproj ./app/
RUN cd app && dotnet restore
COPY . ./app/
RUN cd app && dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ingress-supervisor.dll"]