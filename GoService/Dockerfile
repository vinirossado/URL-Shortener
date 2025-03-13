FROM golang:1.21-alpine AS build

WORKDIR /app
COPY . .
RUN go build -o main .

FROM alpine:latest
WORKDIR /app
COPY --from=build /app/main .

EXPOSE 8080
ENV PORT=8080

# Run the web service on container startup
CMD ["/app/main"]
