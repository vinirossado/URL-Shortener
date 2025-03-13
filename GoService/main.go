package main

import (
	"fmt"
	"log"
	"net/http"
	"os"
)

func main() {
	// Use PORT environment variable provided by App Service or default to 8080
	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	log.Printf("Starting server on port %s", port)

	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		log.Println("Received request on /")
		fmt.Fprintf(w, "Hello World from Go running on Azure App Service!")
	})

	http.HandleFunc("/api/hello", func(w http.ResponseWriter, r *http.Request) {
		log.Println("Received request on /api/hello")
		fmt.Fprintf(w, "Hello from the API endpoint!")
	})

	// Print startup message
	log.Printf("Server starting on port %s", port)

	// Listen on all interfaces on the specified port
	err := http.ListenAndServe(":"+port, nil)
	if err != nil {
		log.Fatalf("Error starting server: %v", err)
	}
}
