# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Full-stack job search application that scrapes Indeed and Monster for full-stack developer positions requiring C#, Azure, and security clearance. Built with ASP.NET Core minimal API (backend) and Vue 3 (frontend).

## Development Commands

### Frontend (JobFinderUI/)
- `npm install` - Install dependencies
- `npm run dev` - Start development server on port 5173 with API proxy to localhost:5000
- `npm run build` - Build for production (outputs to JobFinderApi/wwwroot)
- `npm run preview` - Preview production build

### Backend (JobFinderApi/)
- `dotnet restore` - Restore NuGet packages
- `dotnet run` - Run API server on port 5000
- `dotnet build` - Build project
- `dotnet publish -c Release -o ./publish` - Create production build

### Full Build
- `./build.sh` - Build frontend and prepare for production (runs npm install and build)

## Architecture

### Request Flow
1. User interacts with Vue SPA served from JobFinderApi/wwwroot
2. Frontend makes API calls to `/api/jobs/search/{source}` (source: "indeed", "monster", or "all")
3. JobScrapingService scrapes job sites using HtmlAgilityPack
4. Jobs are filtered for C#, Azure, and security clearance keywords
5. Results returned as JSON to frontend
6. JobListings.vue displays results grouped by source

### Backend Structure (C#)
- **Program.cs**: Minimal API setup with three endpoints:
  - `GET /api/jobs/search/{source}` - Search jobs from specified source
  - `GET /api/jobs/` - Get all cached jobs (currently just calls search with "all")
  - `GET /health` - Health check
  - Fallback route serves index.html for SPA routing
- **Models/JobListing.cs**: Job data model with Id, Title, Company, Location, Description, Url, Source, PostedDate, RequiresSecurityClearance
- **Services/JobScrapingService.cs**:
  - Web scraping implementation using HttpClient and HtmlAgilityPack
  - Separate methods for Indeed and Monster scraping with different CSS selectors
  - Filters jobs for C# + Azure + security clearance keywords
  - Returns top 10 results per source
  - HttpClient configured with 30s timeout and Mozilla User-Agent

### Frontend Structure (Vue 3)
- **App.vue**: Root component managing search state and API calls
- **components/JobSearchForm.vue**: Radio button selection for source (Indeed/Monster/All)
- **components/JobListings.vue**: Groups and displays jobs by source
- **components/JobCard.vue**: Individual job card with title, company, location, description, link
- **vite.config.js**: Build outputs to `../JobFinderApi/wwwroot`, dev server proxies `/api` to port 5000
- Uses Composition API (script setup syntax)
- Tailwind CSS for styling (mobile-first responsive design)

### Key Technical Details
- Frontend builds to backend's wwwroot directory for single-deployment SPA
- CORS enabled with "AllowAll" policy for development
- No database - jobs are scraped on-demand (no persistence)
- Job scraping uses keyword-based filtering (not ML/AI)
- CSS selectors in JobScrapingService are brittle - Indeed/Monster HTML changes will break scraping
- HttpClient is injected via DI with IHttpClientFactory pattern

## Modifying Search Criteria

To change job search parameters, edit [JobScrapingService.cs](JobFinderApi/Services/JobScrapingService.cs):
- Line 57 (Indeed) and line 133 (Monster): Modify `searchTerms` variable
- Lines 87-94 and 163-170: Adjust filtering logic for different tech stacks
- Lines 208-221: Update `clearanceKeywords` array for security clearance detection

## Common Issues

**No jobs found**: Job sites frequently change their HTML structure. Check CSS selectors in JobScrapingService.cs:
- Indeed selectors (lines 67-79): `div.resultContent`, `h2.jobTitle`, `span.companyName`, `div.companyLocation`, `div.job-snippet`
- Monster selectors (lines 143-155): `div.flex-column article`, `h2/a`, `div.company/a`, `div.location`

**CORS errors in development**: Ensure backend is running on port 5000 when using `npm run dev`

**Build fails**: Frontend must build successfully before running backend in production. Use `./build.sh` to ensure proper build order.
