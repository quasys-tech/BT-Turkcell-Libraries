# 1. Aşama: Build (Maven ile derleme)
FROM maven:3.8.5-openjdk-17-slim AS build
COPY src /app/src
COPY pom.xml /app
WORKDIR /app
RUN mvn clean package -DskipTests

# 2. Aşama: Run (Sadece JRE ile çalıştırma)
FROM amazoncorretto:17-alpine
WORKDIR /app

# Derlenen JAR dosyasını kopyala (pom.xml'deki isme göre güncelle)
COPY --from=build /app/target/turkcell-bt-java-lib-*.jar app.jar

# Uygulamayı başlat
ENTRYPOINT ["java", "-cp", "app.jar", "com.turkcell.bt.java.demo.POC"]