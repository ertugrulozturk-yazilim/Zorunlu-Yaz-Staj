# Hata ayıklama kapsayıcınızı özelleştirme ve Visual Studio’nun daha hızlı hata ayıklama için görüntülerinizi derlemek üzere bu Dockerfile'ı nasıl kullandığı hakkında bilgi edinmek için https://aka.ms/customizecontainer sayfasına bakın.

# Bu ARG'ler, VS'den hata ayıklanırken son görüntüyü oluşturmak için kullanılan tabanı değiştirmenizi sağlar
ARG LAUNCHING_FROM_VS
# Bu, LAUNCHING_FROM_VS tanımlanmışsa temel görüntüyü nihai olarak ayarlar
ARG FINAL_BASE_IMAGE=${LAUNCHING_FROM_VS:+aotdebug}

# Bu aşama, VS'den hızlı modda çalıştırıldığında kullanılır (Hata ayıklama yapılandırması için varsayılan olarak ayarlıdır)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Bu aşama, hizmet projesini oluşturmak için kullanılır
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Yerelde yayımlamak için clang/zlib1g-dev bağımlılıklarını yükleyin
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StajyerTakip.API/StajyerTakip.API.csproj", "StajyerTakip.API/"]
RUN dotnet restore "./StajyerTakip.API/StajyerTakip.API.csproj"
COPY . .
WORKDIR "/src/StajyerTakip.API"
RUN dotnet build "./StajyerTakip.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Bu aşama, son aşamaya kopyalanacak hizmet projesini yayımlamak için kullanılır
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StajyerTakip.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# Bu aşama, normal modda hata ayıklamayı desteklemek için VS'den başlatılırken son aşamanın temeli olarak kullanılır (Hata ayıklama yapılandırması kullanılmazken varsayılan olarak ayarlıdır)
FROM base AS aotdebug
USER root
# Yerel hata ayıklamayı desteklemesi için GDB'yi yükleyin
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    gdb
USER app

# Bu aşama üretimde veya VS'den normal modda çalıştırıldığında kullanılır (Hata Ayıklama yapılandırması kullanılmazken varsayılan olarak ayarlıdır)
FROM ${FINAL_BASE_IMAGE:-mcr.microsoft.com/dotnet/runtime-deps:10.0} AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["./StajyerTakip.API"]