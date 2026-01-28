// Background service worker
console.log('Job Finder: Background script loaded');

const API_URL = 'http://localhost:5000/api/jobs';

// Listen for messages from content scripts
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  if (request.action === 'saveJobs') {
    console.log(`Job Finder: Received ${request.jobs.length} jobs from ${request.source}`);

    // Send jobs to API
    saveJobsToAPI(request.jobs)
      .then(result => {
        console.log('Job Finder: Successfully saved jobs to API', result);
        sendResponse({ success: true, count: request.jobs.length });

        // Show notification
        chrome.notifications.create({
          type: 'basic',
          iconUrl: 'icon48.png',
          title: 'Job Finder',
          message: `Saved ${request.jobs.length} matching jobs from ${request.source}`,
          priority: 1
        });
      })
      .catch(error => {
        console.error('Job Finder: Error saving jobs to API', error);
        sendResponse({ success: false, error: error.message });
      });

    return true; // Keep message channel open for async response
  }
});

// Save jobs to the API
async function saveJobsToAPI(jobs) {
  try {
    const response = await fetch(API_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(jobs)
    });

    if (!response.ok) {
      throw new Error(`API returned ${response.status}: ${response.statusText}`);
    }

    const result = await response.json();
    return result;
  } catch (error) {
    console.error('Job Finder: Failed to save jobs to API:', error);

    // If API is not available, store locally
    console.log('Job Finder: API not available, storing locally');
    await saveJobsLocally(jobs);
    throw error;
  }
}

// Fallback: Save jobs to Chrome storage
async function saveJobsLocally(jobs) {
  try {
    const result = await chrome.storage.local.get(['savedJobs']);
    const existingJobs = result.savedJobs || [];

    // Merge new jobs with existing ones (avoid duplicates by URL)
    const jobMap = new Map();
    existingJobs.forEach(job => jobMap.set(job.url, job));
    jobs.forEach(job => jobMap.set(job.url, job));

    const allJobs = Array.from(jobMap.values());

    await chrome.storage.local.set({ savedJobs: allJobs });
    console.log(`Job Finder: Saved ${allJobs.length} total jobs locally`);
  } catch (error) {
    console.error('Job Finder: Failed to save jobs locally:', error);
  }
}

// Handle extension icon click
chrome.action.onClicked.addListener((tab) => {
  // Open the Job Finder web app
  chrome.tabs.create({ url: 'http://localhost:5173' });
});
