# learnwasm

A Blazor Server web application built with .NET 10 that provides a web interface for the Learn MCP Server.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Getting Started

```bash
dotnet run
```

The app will be available at `https://localhost:5001` (or the port shown in the terminal).

## Project Structure

```
Components/
  Layout/       # Shared layout components (MainLayout, NavMenu)
  Pages/        # Page components (Home, Counter, Weather, Error, NotFound)
  App.razor     # Root application component
wwwroot/        # Static assets (CSS, images, JS libs)
Program.cs      # Application entry point and service configuration
```

## Pages

| Page | Route | Description |
|------|-------|-------------|
| Home | `/` | Landing page |
| Counter | `/counter` | Interactive counter demo |
| Weather | `/weather` | Weather data demo |

## Tech Stack

- **Blazor Server** with Interactive Server render mode
- **.NET 10**
- **Razor Components**
