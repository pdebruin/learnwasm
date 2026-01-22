# learnwasm

A Blazor WebAssembly application for searching Microsoft Learn documentation using the MCP (Model Context Protocol) server.

## 🚀 Live Demo

The app is automatically deployed to GitHub Pages: [https://pdebruin.github.io/learnwasm/](https://pdebruin.github.io/learnwasm/)

## 🔧 Development

### Prerequisites
- .NET 10 SDK

### Running Locally
```bash
cd src
dotnet run
```

### Building for Production
```bash
cd src
dotnet publish -c Release -o ../publish
```

## 🤖 CI/CD

This repository uses GitHub Actions for continuous integration and deployment:

- **Build & Test**: Automatically builds and tests the application on every push to main
- **Deploy**: Publishes the app to GitHub Pages
- **Verify**: Checks that the deployment is accessible and working

### GitHub Pages Setup

To enable GitHub Pages deployment for your own fork:

1. Go to your repository Settings
2. Navigate to Pages (under Code and automation)
3. Under "Build and deployment", select:
   - Source: GitHub Actions
4. The workflow will automatically deploy on the next push to main

## 📝 Features

- Search Microsoft Learn documentation
- Interactive Blazor WebAssembly UI
- Bootstrap styling
- Weather forecast demo
- Counter demo
