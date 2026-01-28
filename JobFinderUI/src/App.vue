<script setup>
import { ref, computed, onMounted } from 'vue'
import JobSearchForm from './components/JobSearchForm.vue'
import JobListings from './components/JobListings.vue'

const jobs = ref([])
const loading = ref(false)
const error = ref('')
const selectedSource = ref('all')
const isDarkMode = ref(false)

const filteredJobs = computed(() => {
  if (!selectedSource.value || selectedSource.value === 'all') {
    return jobs.value
  }
  return jobs.value.filter(job =>
    job.source.toLowerCase() === selectedSource.value.toLowerCase()
  )
})

const handleSearch = async (source) => {
  loading.value = true
  error.value = ''

  try {
    const response = await fetch(`/api/jobs/search/${source}`)
    if (!response.ok) {
      throw new Error('Failed to fetch jobs')
    }
    const data = await response.json()
    jobs.value = data.jobs || []
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
}

const toggleDarkMode = () => {
  isDarkMode.value = !isDarkMode.value
  console.log('Toggle dark mode:', isDarkMode.value)

  if (isDarkMode.value) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('theme', 'light')
  }

  console.log('HTML classes:', document.documentElement.classList.toString())
}

onMounted(() => {
  // Load theme preference from localStorage
  const savedTheme = localStorage.getItem('theme')
  console.log('Saved theme:', savedTheme)

  if (savedTheme === 'dark' || (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    isDarkMode.value = true
    document.documentElement.classList.add('dark')
    console.log('Initialized to dark mode')
  } else {
    isDarkMode.value = false
    document.documentElement.classList.remove('dark')
    console.log('Initialized to light mode')
  }

  // Optional: Load jobs on mount
  // handleSearch('all')
})
</script>

<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 transition-colors duration-200">
    <!-- Header -->
    <header class="bg-white dark:bg-gray-800 shadow-sm sticky top-0 z-10">
      <div class="max-w-4xl mx-auto px-4 py-4 sm:px-6 sm:py-6">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl sm:text-3xl font-bold text-gray-900 dark:text-white">
              💼 Job Finder
            </h1>
            <p class="text-gray-600 dark:text-gray-300 text-sm sm:text-base mt-1">
              Find Full-Stack Developer roles requiring security clearance
            </p>
          </div>
          <!-- Theme Toggle Button -->
          <button
            @click="toggleDarkMode"
            class="p-2 rounded-lg bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors duration-200"
            :aria-label="isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'"
          >
            <svg v-if="!isDarkMode" class="w-6 h-6 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
            </svg>
            <svg v-else class="w-6 h-6 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
          </button>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="max-w-4xl mx-auto px-4 py-6 sm:px-6 sm:py-8">
      <!-- Search Form -->
      <JobSearchForm
        @search="handleSearch"
        :loading="loading"
        @source-change="selectedSource = $event"
      />

      <!-- Error Message -->
      <div v-if="error" class="mt-6 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
        <p class="text-red-800 dark:text-red-300 text-sm sm:text-base">{{ error }}</p>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="mt-6 flex justify-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 dark:border-indigo-400"></div>
      </div>

      <!-- Job Listings -->
      <JobListings
        v-if="!loading && jobs.length > 0"
        :jobs="filteredJobs"
        :total-count="jobs.length"
      />

      <!-- Empty State -->
      <div v-if="!loading && jobs.length === 0" class="mt-8 text-center">
        <p class="text-gray-600 dark:text-gray-300 text-base sm:text-lg">
          No jobs found. Search to get started!
        </p>
      </div>
    </main>

    <!-- Footer -->
    <footer class="bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 mt-12">
      <div class="max-w-4xl mx-auto px-4 py-6 sm:px-6 text-center text-gray-600 dark:text-gray-300 text-sm">
        <p>Job listings are sourced from Indeed and Monster</p>
      </div>
    </footer>
  </div>
</template>
