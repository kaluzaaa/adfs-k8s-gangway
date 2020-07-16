FROM mcr.microsoft.com/dotnet/core/sdk:3.1

COPY m.crt /usr/local/share/ca-certificates/

RUN chmod 644 /usr/local/share/ca-certificates/m.crt && update-ca-certificates

WORKDIR /source

COPY *.csproj .
RUN ["dotnet", "restore"]

COPY . .
RUN ["dotnet", "build"]

EXPOSE 3000/tcp

ENTRYPOINT ["dotnet", "run"]
