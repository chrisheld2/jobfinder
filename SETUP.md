# Setup Guide - Job Finder

This guide will walk you through setting up and running the Job Finder application.

## Prerequisites

Before you begin, ensure you have:
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** - [Download](https://nodejs.org)
- **npm** (comes with Node.js)

Verify installations:
```bash
dotnet --version
node --version
npm --version
```

## Quick Start

### Option 1: Using the Build Script (Recommended)

```bash
# Make the script executable
chmod +x build.sh

# Run the build script
./build.sh

# Start the API
cd JobFinderApi
dotnet run
```

Then visit `http://localhost:5000` in your browser.

### Option 2: Manual Setup

#### 1. Build the Frontend

```bash
cd JobFinderUI
npm install
npm run build
```

This creates optimized files in `JobFinderApi/wwwroot`.

#### 2. Run the Backend

```bash
cd JobFinderApi
dotnet run
```

Visit `http://localhost:5000` in your browser.

## Development Setup

If you want to develop with hot module replacement:

### Terminal 1: Frontend Development Server

```bash
cd JobFinderUI
npm install
npm run dev
```

Visit `http://localhost:5173`

### Terminal 2: Backend API

```bash
cd JobFinderApi
dotnet run
```

The frontend dev server automatically proxies API calls to `http://localhost:5000/api`.

## How It Works

### Architecture

```
┌─────────────────────────────────────┐
│     Vue 3 Frontend (SPA)            │
│  - JobSearchForm.vue                │
│  - JobListings.vue                  │
│  - JobCard.vue                      │
└──────────────┬──────────────────────┘
               │ HTTP Requests
               ▼
┌─────────────────────────────────────┐
│   ASP.NET Core Minimal API          │
│  - /api/jobs/search/{source}        │
│  - /api/jobs/                       │
│  - /health                          │
└──────────────┬──────────────────────┘
               │ Web Scraping
               ▼
       Indeed & Monster Sites
```

### Data Flow

1. **User clicks "Search Jobs"** in the Vue frontend
2. **JobSearchForm.vue** emits a search event with the selected source
3. **App.vue** makes an API call to `/api/jobs/search/{source}`
4. **JobScrapingService** scrapes Indeed/Monster for matching jobs
5. **Filtering** applies these criteria:
   - Title or description contains "C#"
   - Title or description contains "Azure"
   - Title or description contains security clearance keywords
6. **Results** are returned as JSON
7. **JobListings.vue** and **JobCard.vue** display the results

## Deployment

### Build for Production

```bash
# Build frontend
cd JobFinderUI
npm run build
cd ..

# Publish backend
cd JobFinderApi
dotnet publish -c Release -o ./publish
```

### Deploy

Copy the contents of `JobFinderApi/publish` to your server and run:

```bash
cd JobFinderApi
./JobFinderApi  # On Linux/Mac
JobFinderApi.exe  # On Windows
```

## Troubleshooting

### Port Already in Use

If port 5000 is already in use:

**For backend:**
```bash
dotnet run --urls "https://localhost:5001"
```

**For frontend development:**
```bash
npm run dev -- --port 5174
```

### npm install Issues

If you get permission errors:

```bash
npm cache clean --force
npm install
```

### Build Fails

Clear NuGet cache:
```bash
dotnet nuget locals all --clear
dotnet restore
```

### No Jobs Found

This can happen if:
1. Indeed/Monster HTML structure has changed
2. Network is slow (increase timeout in JobScrapingService)
3. Search criteria are too restrictive

Check browser console and API logs for errors.

## Environment Configuration

Currently, the application uses hardcoded job search terms. To customize:

Edit `JobFinderApi/Services/JobScrapingService.cs`:

```csharp
var searchTerms = "full-stack developer C# Azure";  // Change this
```

## Understanding the Code

### Backend Structure

- **Models/JobListing.cs** - Job data model
- **Services/JobScrapingService.cs** - Web scraping logic
- **Program.cs** - API configuration and endpoints

### Frontend Structure

- **App.vue** - Main component, state management
- **components/JobSearchForm.vue** - Search interface
- **components/JobListings.vue** - Results container
- **components/JobCard.vue** - Individual job display
- **style.css** - Tailwind CSS imports
- **main.js** - Vue app initialization

## Next Steps

1. ✅ Run the application
2. 🔍 Search for jobs
3. 📱 Test on mobile device
4. 🔧 Customize search criteria
5. 🚀 Deploy to your server

## Support

For issues:
1. Check the browser console (F12)
2. Check the API logs in the terminal
3. Verify Indeed/Monster website structure hasn't changed
4. Check internet connection and proxy settings
