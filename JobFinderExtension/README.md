# Job Finder Chrome Extension

This Chrome extension automatically collects job listings from Indeed and Monster while you browse, filtering for full-stack C#/Azure positions that require security clearance.

## Features

- 🔍 **Automatic Detection**: Scans job listings as you browse Indeed and Monster
- 🎯 **Smart Filtering**: Only saves jobs matching your criteria (C#/Azure + Security Clearance)
- 💾 **Local Storage**: Sends jobs to your local API for persistent storage
- 🔔 **Notifications**: Shows confirmation when jobs are saved
- 🖱️ **Manual Control**: Click extension icon to trigger manual scan

## Installation

### 1. Load Extension in Chrome

1. Open Chrome and navigate to `chrome://extensions/`
2. Enable "Developer mode" (toggle in top-right corner)
3. Click "Load unpacked"
4. Select the `JobFinderExtension` folder
5. The extension should now appear in your extensions list

### 2. Start the API

Make sure your Job Finder API is running:

```bash
cd JobFinderApi
dotnet run
```

The API should be running on `http://localhost:5000`

### 3. Start the Frontend (Optional)

If you want to view collected jobs in the web UI:

```bash
cd JobFinderUI
npm run dev
```

The UI will be available at `http://localhost:5173`

## How to Use

### Automatic Collection

1. Navigate to Indeed or Monster job search pages
2. Search for any full-stack developer positions
3. The extension automatically scans the page for matching jobs
4. Matching jobs are sent to your API and stored
5. A notification confirms when jobs are saved

**Example searches:**
- https://www.indeed.com/jobs?q=full+stack+developer
- https://www.monster.com/jobs/search?q=software+engineer

### Manual Scan

1. Click the extension icon in your Chrome toolbar
2. Click "Scan Current Page" to manually trigger extraction
3. Or click "Open Job Finder App" to view collected jobs

### Viewing Collected Jobs

1. Click the extension icon and select "Open Job Finder App"
2. Or navigate directly to `http://localhost:5173`
3. All collected jobs will be displayed grouped by source

## How It Works

1. **Content Scripts** (`content-indeed.js`, `content-monster.js`):
   - Run on Indeed/Monster pages
   - Extract job data from the fully-rendered DOM
   - Filter jobs based on criteria (C#, Azure, clearance)
   - Send matching jobs to background script

2. **Background Script** (`background.js`):
   - Receives jobs from content scripts
   - Posts jobs to API endpoint (`POST /api/jobs`)
   - Shows browser notifications
   - Handles errors and fallbacks

3. **API** (JobFinderApi):
   - Receives jobs via POST endpoint
   - Stores jobs in memory (survives across page loads)
   - Serves jobs to frontend via GET endpoint

4. **Frontend** (JobFinderUI):
   - Vue 3 app displays all collected jobs
   - Shows jobs grouped by source
   - Provides links to original job postings

## Filtering Criteria

Jobs must meet ALL of these requirements to be saved:

- ✅ Contains "full" AND "stack" in title or description
- ✅ Contains "C#" OR "Azure" in title or description
- ✅ Contains security clearance keywords:
  - "security clearance"
  - "secret clearance"
  - "top secret"
  - "TS/SCI"
  - "clearance required"
  - "active clearance"
  - "clearable"

## Troubleshooting

### Extension not collecting jobs

1. Check browser console (F12) for error messages
2. Verify API is running on port 5000
3. Try clicking "Scan Current Page" manually
4. Refresh the Indeed/Monster page

### API connection errors

1. Ensure API is running: `dotnet run` in JobFinderApi folder
2. Check API is accessible: `http://localhost:5000/health`
3. Verify CORS is enabled (already configured)

### No jobs found

1. The filtering is strict - jobs must match ALL criteria
2. Try broader searches on Indeed/Monster
3. Check console logs to see which jobs were examined
4. Look for messages like "Indeed job: [title] - FullStack:true C#:true Azure:false Clearance:true"

## Technical Details

- **Manifest Version**: 3
- **Permissions**: storage, activeTab
- **Host Permissions**: indeed.com, monster.com
- **Run At**: document_idle (waits for page to load)
- **Storage**: In-memory on API (ConcurrentDictionary)

## Development

To modify the extension:

1. Edit files in `JobFinderExtension/`
2. Go to `chrome://extensions/`
3. Click the refresh icon on the Job Finder Helper extension
4. Reload any Indeed/Monster tabs to pick up changes

## Privacy

This extension:
- ✅ Only runs on Indeed and Monster
- ✅ Only stores data locally (your machine)
- ✅ Only sends data to localhost:5000 (your API)
- ✅ Does not send data to any external servers
- ✅ Does not collect personal information
