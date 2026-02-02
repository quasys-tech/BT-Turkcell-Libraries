# 1. Aşama: Build
FROM maven:3.8.5-openjdk-17-slim AS build
WORKDIR /app
COPY pom-demo.xml ./pom.xml 
COPY src ./src
# ÖNEMLİ: -f parametresiyle özel pom dosyasını gösteriyoruz
RUN mvn -f pom.xml clean package -DskipTests

# 2. Aşama: Run
FROM amazoncorretto:17-alpine
WORKDIR /app
COPY --from=build /app/target/*.jar app.jar

ENTRYPOINT ["java", "-jar", "app.jar"]