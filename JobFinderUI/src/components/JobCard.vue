<script setup>
import { computed } from 'vue'

const props = defineProps({
  job: {
    type: Object,
    required: true
  }
})

const formattedDate = computed(() => {
  if (!props.job.postedDate) return 'Date unknown'
  const date = new Date(props.job.postedDate)
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
})
</script>

<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow hover:shadow-md dark:hover:shadow-lg transition-all duration-200 p-4 sm:p-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-3 mb-3">
      <div class="flex-1 min-w-0">
        <h3 class="text-base sm:text-lg font-semibold text-gray-900 dark:text-white truncate hover:text-clip">
          {{ job.title }}
        </h3>
        <p class="text-sm sm:text-base text-gray-600 dark:text-gray-300 mt-1">
          {{ job.company }}
        </p>
      </div>
      <div class="flex-shrink-0">
        <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs sm:text-sm font-medium bg-red-100 dark:bg-red-900/40 text-red-800 dark:text-red-300">
          🔐 Clearance
        </span>
      </div>
    </div>

    <!-- Location and Date -->
    <div class="flex flex-col sm:flex-row sm:items-center gap-2 sm:gap-4 text-xs sm:text-sm text-gray-600 dark:text-gray-400 mb-4">
      <div class="flex items-center">
        <span class="mr-1">📍</span>
        <span>{{ job.location }}</span>
      </div>
      <div class="flex items-center">
        <span class="mr-1">📅</span>
        <span>{{ formattedDate }}</span>
      </div>
    </div>

    <!-- Description -->
    <p class="text-sm sm:text-base text-gray-700 dark:text-gray-300 line-clamp-3 mb-4">
      {{ job.description }}
    </p>

    <!-- View Job Button -->
    <a
      :href="job.url"
      target="_blank"
      rel="noopener noreferrer"
      class="inline-flex items-center justify-center w-full sm:w-auto px-4 py-2 bg-indigo-600 hover:bg-indigo-700 dark:bg-indigo-500 dark:hover:bg-indigo-600 text-white text-sm sm:text-base font-medium rounded-lg transition-colors duration-200"
    >
      View Job on {{ job.source }}
      <span class="ml-2">→</span>
    </a>
  </div>
</template>
