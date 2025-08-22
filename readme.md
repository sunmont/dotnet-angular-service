\# E-commerce Application

This is a full-stack e-commerce application built with .NET Core, Angular, and other related technologies. The application showcases a standard e-commerce platform with product listings, a shopping basket, and user authentication.

## Technologies Used

### Backend

*   **.NET 7**
*   **ASP.NET Core Web API**
*   **Entity Framework Core**
*   **SQLite**: Used as the database for both the main application and identity management.
*   **JWT Authentication**: For securing the API.
*   **Kafka**: For messaging between services.
*   **Redis**: For managing the shopping basket.
*   **AutoMapper**: For object-to-object mapping.

### Frontend

*   **Angular 16**
*   **Bootstrap**: For styling and UI components.
*   **ngx-bootstrap**: Angular widgets for Bootstrap.
*   **ngx-spinner**: For loading indicators.
*   **ngx-toastr**: For notifications.

### Infrastructure

*   **Docker & Docker Compose**: For containerizing and running the application services.

## Getting Started

### Prerequisites

*   [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
*   [Node.js](https://nodejs.org/) (which includes npm)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Running with Docker Compose

The easiest way to get the application running is by using Docker Compose. This will start the Redis container required for the application.

1.  Open a terminal in the root of the project.
2.  Run the following command:

    ```bash
    docker-compose up
    ```

This will start the Redis container. After that, you will need to run the API and the Client application separately.

### Running the API

1.  Navigate to the `API` directory:
    ```bash
    cd API
    ```
2.  Run the application:
    ```bash
    dotnet run
    ```
The API will be available at `https://localhost:5001`.

### Running the Client

1.  Navigate to the `Client` directory:
    ```bash
    cd Client
    ```
2.  Install the dependencies:
    ```bash
    npm install
    ```
3.  Run the application:
    ```bash
    ng serve
    ```
The client application will be available at `http://localhost:4200`.

## Project Structure

The solution is divided into four main projects:

*   **`API`**: The ASP.NET Core Web API project that serves the data to the client.
*   **`Client`**: The Angular frontend application.
*   **`Core`**: A class library containing the core entities, interfaces, and business logic.
*   **`Infrastructure`**: A class library containing the data access logic, repositories, and other infrastructure concerns.

## API Endpoints

The API provides the following main endpoints:

*   `api/products`: For getting products.
*   `api/basket`: For managing the shopping basket.
*   `api/account`: For user registration and login.
*   `api/orders`: For managing orders (via Kafka).

For a full list of endpoints, you can explore the Swagger documentation available at `https://localhost:5001/swagger`.
