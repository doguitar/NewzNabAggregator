FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /app

COPY . ./
WORKDIR /app
RUN dotnet publish -c Docker -o out



# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
  
ENV ASPNETCORE_URLS="http://+:5000"
ENV ASPNETCORE_ENVIRONMENT="Docker"
ENV NNA_baseDir="/config"
EXPOSE 5000
VOLUME /config 

WORKDIR /app
COPY --from=build /app/out .

RUN  mkdir -p /config  \
  && chmod 777 /config 

COPY --from=build /app/out/appsettings.Docker.json /config/appsettings.json
RUN ls -la /config

#ENTRYPOINT ["ls", "-la", "/config"]

ENTRYPOINT ["/app/NewzNabAggregator.Web", \
  "--config", "/config/appsettings.json"]
