FROM golang:1.21-alpine AS build

WORKDIR /app
COPY . .
RUN go build -o main .

FROM alpine:latest
WORKDIR /app
COPY --from=build /app/main .

# Expose port 80 for App Service (important!)
EXPOSE 80

# Set environment variable for the port
ENV PORT=80

# Run the web service on container startup
CMD ["/app/main"]
