// Content script for Monster.com
console.log('Job Finder: Monster content script loaded');

// Extract job data from Monster search results page
function extractMonsterJobs() {
  const jobs = [];

  // Try multiple selectors for job cards
  const jobCardSelectors = [
    'section.card-content',
    'div.job-card',
    'article[data-testid="job-card"]',
    'div[class*="JobCard"]',
    'div.card'
  ];

  let jobCards = [];
  for (const selector of jobCardSelectors) {
    jobCards = document.querySelectorAll(selector);
    if (jobCards.length > 0) {
      console.log(`Job Finder: Found ${jobCards.length} job cards using selector: ${selector}`);
      break;
    }
  }

  if (jobCards.length === 0) {
    console.log('Job Finder: No job cards found on Monster');
    return jobs;
  }

  jobCards.forEach((card, index) => {
    try {
      // Extract title
      const titleElement = card.querySelector('h2 a') ||
                          card.querySelector('a[data-test-id="svx-job-title"]') ||
                          card.querySelector('a.title') ||
                          card.querySelector('h3 a');

      // Extract company
      const companyElement = card.querySelector('div.company a') ||
                            card.querySelector('span.company') ||
                            card.querySelector('[data-test-id="svx-job-company"]') ||
                            card.querySelector('div[class*="company"]');

      // Extract location
      const locationElement = card.querySelector('div.location') ||
                             card.querySelector('[data-test-id="svx-job-location"]') ||
                             card.querySelector('div[class*="location"]');

      // Extract description
      const descriptionElement = card.querySelector('div.summary') ||
                                card.querySelector('div.description') ||
                                card.querySelector('[data-test-id="svx-job-description"]') ||
                                card.querySelector('div[class*="snippet"]');

      // Extract URL
      const linkElement = card.querySelector('h2 a') ||
                         card.querySelector('a[data-test-id="svx-job-title"]');

      if (titleElement && companyElement) {
        const title = titleElement.innerText.trim();
        const company = companyElement.innerText.trim();
        const location = locationElement ? locationElement.innerText.trim() : 'Remote';
        const description = descriptionElement ? descriptionElement.innerText.trim() : '';
        let url = linkElement ? linkElement.href : window.location.href;

        // Make sure URL is absolute
        if (url && !url.startsWith('http')) {
          url = 'https://www.monster.com' + url;
        }

        // Check if job matches our criteria
        const fullText = `${title} ${description}`.toLowerCase();
        const hasCSharp = fullText.includes('c#') || fullText.includes('.net');
        const hasAzure = fullText.includes('azure');
        const isFullStack = fullText.includes('full') && fullText.includes('stack');
        const hasClearance = /security clearance|secret clearance|top secret|ts\/sci|clearance required|active clearance|clearable/i.test(fullText);

        if (isFullStack && (hasCSharp || hasAzure) && hasClearance) {
          jobs.push({
            title,
            company,
            location,
            description,
            url,
            source: 'Monster',
            postedDate: new Date().toISOString(),
            requiresSecurityClearance: true
          });

          console.log(`Job Finder: Extracted job ${jobs.length}: ${title} at ${company}`);
        }
      }
    } catch (error) {
      console.error('Job Finder: Error extracting job card', index, error);
    }
  });

  return jobs;
}

// Send jobs to background script
function sendJobsToBackground(jobs) {
  if (jobs.length > 0) {
    chrome.runtime.sendMessage({
      action: 'saveJobs',
      jobs: jobs,
      source: 'Monster'
    }, (response) => {
      if (chrome.runtime.lastError) {
        console.error('Job Finder: Error sending jobs:', chrome.runtime.lastError);
      } else {
        console.log(`Job Finder: Sent ${jobs.length} jobs to background script`);
      }
    });
  } else {
    console.log('Job Finder: No matching jobs found on this page');
  }
}

// Extract jobs when page loads
function init() {
  console.log('Job Finder: Initializing Monster extraction');

  // Wait for page to be fully loaded
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
      setTimeout(() => {
        const jobs = extractMonsterJobs();
        sendJobsToBackground(jobs);
      }, 2000);
    });
  } else {
    // Page already loaded
    setTimeout(() => {
      const jobs = extractMonsterJobs();
      sendJobsToBackground(jobs);
    }, 2000);
  }
}

// Run extraction
init();

// Also watch for dynamic content changes (pagination, infinite scroll)
let debounceTimer;
const observer = new MutationObserver(() => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    const jobs = extractMonsterJobs();
    if (jobs.length > 0) {
      sendJobsToBackground(jobs);
    }
  }, 3000);
});

observer.observe(document.body, {
  childList: true,
  subtree: true
});
