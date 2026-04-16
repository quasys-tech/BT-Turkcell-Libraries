FROM maven:3.9.9-eclipse-temurin-17 AS build
WORKDIR /src

COPY pom.xml ./
COPY pom-demo.xml ./
COPY src ./src

RUN mvn -f pom-demo.xml -DskipTests package

FROM eclipse-temurin:17-jre
WORKDIR /app
COPY --from=build /src/target/*-shaded.jar /app/app.jar

ENTRYPOINT ["java", "-jar", "/app/app.jar"]
