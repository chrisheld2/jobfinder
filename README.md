# Job Finder - Full-Stack Developer Security Clearance Jobs

A full-stack web application built with C# ASP.NET Core minimal API and Vue 3 that searches USAJobs for full-stack developer positions requiring C#, Azure expertise, and security clearance.

## Features

- 🔍 Search jobs from USAJobs (official U.S. Government job board)
- 🔐 Filter for security clearance requirements
- 💻 Mobile-first responsive design with Tailwind CSS
- 🌙 Dark/Light mode theme toggle with localStorage persistence
- ⚡ Vue 3 with Composition API (Setup mode)
- 🎯 Minimal ASP.NET Core API
- 🚀 Single-page application served from the API

## Project Structure

```
Job Finder/
├── JobFinderApi/              # C# ASP.NET Core API
│   ├── Models/
│   │   └── JobListing.cs
│   ├── Services/
│   │   └── JobScrapingService.cs
│   ├── Program.cs
│   ├── JobFinderApi.csproj
│   └── wwwroot/              # Static files (built Vue app)
│
├── JobFinderUI/              # Vue 3 Frontend
│   ├── src/
│   │   ├── components/
│   │   │   ├── JobSearchForm.vue
│   │   │   ├── JobListings.vue
│   │   │   └── JobCard.vue
│   │   ├── App.vue
│   │   ├── main.js
│   │   └── style.css
│   ├── index.html
│   ├── package.json
│   ├── vite.config.js
│   ├── tailwind.config.js
│   └── postcss.config.js
│
└── README.md
```

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm

## Setup Instructions

### 1. Backend Setup

```bash
cd JobFinderApi
dotnet restore
```

### 2. Frontend Setup

```bash
cd JobFinderUI
npm install
```

## Development

### Run Frontend Development Server

```bash
cd JobFinderUI
npm run dev
```

The frontend will be available at `http://localhost:5173` with API proxying to `http://localhost:5000`.

### Run Backend API

```bash
cd JobFinderApi
dotnet run
```

The API will be available at `http://localhost:5000`.

### Using Real Job Data (Optional)

By default, the app returns sample job listings. To search real USAJobs data:

1. Sign up for a free API key at https://developer.usajobs.gov/APIRequest/Index
2. Set the environment variable:
   ```bash
   export USAJOBS_API_KEY="your-api-key-here"
   ```
3. Restart the API

Without an API key, the app will display 7 sample government jobs for demonstration purposes.

### Build Frontend for Production

```bash
cd JobFinderUI
npm run build
```

This builds the Vue app and outputs to `JobFinderApi/wwwroot`.

## API Endpoints

### Search Jobs

```
GET /api/jobs/search/{source}
```

**Parameters:**
- `source`: Any string (currently only USAJobs is supported, but the parameter is kept for future expansion)

**Response:**
```json
{
  "jobs": [
    {
      "id": "guid",
      "title": "Senior Full-Stack Developer - Secret Clearance",
      "company": "Department of Defense",
      "location": "Arlington, VA",
      "description": "Develop and maintain mission-critical applications...",
      "url": "https://www.usajobs.gov/job/...",
      "source": "USAJobs",
      "postedDate": "2026-01-25T12:00:00Z",
      "requiresSecurityClearance": true
    }
  ],
  "count": 7,
  "timestamp": "2026-01-27T12:00:00Z"
}
```

### Get All Cached Jobs

```
GET /api/jobs/
```

### Health Check

```
GET /health
```

## Features

### Job Search Service

The `JobScrapingService` uses the USAJobs API (official U.S. Government job board) to search for:
- Full-stack developer positions
- C# and Azure keywords
- Security clearance requirements
- Government jobs that often require security clearances
- Filters results to only include relevant positions

**Note:** The service returns sample jobs by default. Set the `USAJOBS_API_KEY` environment variable to fetch real job listings.

### Frontend Components

- **JobSearchForm**: Radio button selection for job source, search button
- **JobListings**: Groups jobs by source, displays count
- **JobCard**: Individual job details with link to original posting

### Responsive Design

- Mobile-first approach with Tailwind CSS
- Works seamlessly on:
  - Mobile devices (320px+)
  - Tablets (768px+)
  - Desktop (1024px+)
- Optimized touch targets and spacing
- Readable typography scales with screen size

## Building for Production

### Build Frontend

```bash
cd JobFinderUI
npm run build
```

### Publish Backend

```bash
cd JobFinderApi
dotnet publish -c Release -o ./publish
```

Then deploy the `publish` folder to your server.

## Notes

- The job search service uses the official USAJobs API, which is legal and designed for programmatic access
- By default, the app returns 7 sample jobs for demonstration. Get a free API key from https://developer.usajobs.gov/ for real data
- USAJobs focuses on U.S. Government positions, which commonly require security clearances
- Security clearance detection is keyword-based and may have false positives/negatives
- Consider implementing caching to reduce API calls and improve performance

## Future Enhancements

- Database integration for job caching
- Search history and saved jobs
- Email notifications for new postings
- Advanced filtering options
- User authentication
- Job application tracking
