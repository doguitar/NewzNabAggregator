FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /app

COPY . ./
WORKDIR /app
RUN dotnet publish -c Release -o out



# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
RUN  mkdir -p /config  \
  && chmod 777 /config 
  
ENV ASPNETCORE_URLS="http://+:5000"
ENV ASPNETCORE_ENVIRONMENT Docker
EXPOSE 5000
VOLUME /config 

RUN cp -n /app/appsettings.Docker.json /config/appsettings.json

WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "NewzNabAggregator.Web" \
  "--config", "/config/appsettings.json"]
