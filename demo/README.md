# Flagship Demo Asp.Net core Application

Welcome to the Flagship Demo Asp.Net core Application. This application is a demonstration of how to use Flagship for feature flagging and A/B testing in an Asp.Net core application.

This implementation is based on two use cases:

1. **Fs demo toggle use case**: This feature toggle campaign enables a discount for VIP users.
2. **Fs demo A/B Test use case**: This A/B test campaign allows you to test the color of the 'Add to Cart' button.

## Prerequisites

Before you begin, ensure you have met the following requirements:

- You have installed the latest version of [.NET SDK](https://dotnet.microsoft.com/download)
- You have [Docker](https://www.docker.com/products/docker-desktop) installed (optional)
- [Flagship account](https://www.abtasty.com)

## Getting Started

### Running the Application Locally

Follow these steps to get up and running quickly on your local machine:

1. **Install the .NET SDK**: Ensure you have the latest version of the .NET SDK installed. You can download it from the [.NET download page](https://dotnet.microsoft.com/download).

2. **Clone the Repository**: Clone the repository containing the Flagship demo ASP.NET Core application.

    ```bash
    git clone https://github.com/flagship-io/flagship-dotnet-sdk.git
    cd flagship-dotnet-sdk/demo
    ```

3. **Restore Dependencies**: Restore the required dependencies for the project.

    ```bash
    dotnet restore
    ```

4. **Build the Application**: Build the application to ensure all dependencies are correctly set up.

    ```bash
    dotnet build
    ```

5. **Run the Application**: Run the application locally.

    ```bash
    dotnet run
    ```

The application will be accessible at `http://localhost:5001`.

### Running the Application in Docker

If you prefer to use Docker, you can build and run the application using the provided shell script:

```bash
chmod +x run-docker.sh && ./run-docker.sh
```

## API Endpoints

This application provides the following API endpoints:

### GET /item

This endpoint fetches an item and applies any feature flags for the visitor.

Example:

```bash
curl http://localhost:5000/item
```

This will return a JSON object with the item details and any modifications applied by feature flags.

### POST /add-to-cart

This endpoint simulates adding an item to the cart and sends a hit to track the action.

Example:

 ```bash
 curl -X POST http://localhost:5000/add-to-cart
 ```

 This will send a hit to track the "add-to-cart-clicked" action for the visitor.
