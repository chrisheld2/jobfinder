// Popup script
document.getElementById('openApp').addEventListener('click', () => {
  chrome.tabs.create({ url: 'http://localhost:5173' });
});

document.getElementById('scanPage').addEventListener('click', async () => {
  const statusEl = document.getElementById('status');
  statusEl.textContent = 'Scanning current page...';
  statusEl.className = 'status';
  statusEl.style.display = 'block';

  try {
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });

    if (tab.url.includes('indeed.com') || tab.url.includes('monster.com')) {
      // Reload the page to trigger content script
      chrome.tabs.reload(tab.id);

      setTimeout(() => {
        statusEl.textContent = 'Page refreshed! Check console for results.';
        statusEl.className = 'status success';
      }, 500);
    } else {
      statusEl.textContent = 'Please navigate to Indeed or Monster to scan for jobs.';
      statusEl.className = 'status error';
    }
  } catch (error) {
    statusEl.textContent = 'Error: ' + error.message;
    statusEl.className = 'status error';
  }
});
