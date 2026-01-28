<script setup>
import { computed } from 'vue'
import JobCard from './JobCard.vue'

const props = defineProps({
  jobs: {
    type: Array,
    required: true
  },
  totalCount: {
    type: Number,
    required: true
  }
})

const jobsBySource = computed(() => {
  const grouped = {}
  props.jobs.forEach(job => {
    if (!grouped[job.source]) {
      grouped[job.source] = []
    }
    grouped[job.source].push(job)
  })
  return grouped
})
</script>

<template>
  <div class="mt-8">
    <!-- Results Summary -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-4 sm:p-6 mb-6 transition-colors duration-200">
      <h2 class="text-xl sm:text-2xl font-bold text-gray-900 dark:text-white">
        Results
        <span class="text-indigo-600 dark:text-indigo-400">({{ jobs.length }})</span>
      </h2>
      <p class="text-gray-600 dark:text-gray-300 text-sm sm:text-base mt-1">
        Found {{ jobs.length }} of {{ totalCount }} matching job{{ totalCount !== 1 ? 's' : '' }}
      </p>
    </div>

    <!-- Jobs by Source -->
    <div class="space-y-6 sm:space-y-8">
      <div
        v-for="(sourceJobs, sourceName) in jobsBySource"
        :key="sourceName"
        class="space-y-4"
      >
        <!-- Source Header -->
        <div class="flex items-center space-x-3 px-2">
          <span class="inline-flex items-center px-3 py-1 rounded-full text-xs sm:text-sm font-medium bg-indigo-100 dark:bg-indigo-900/40 text-indigo-800 dark:text-indigo-300">
            {{ sourceName }}
          </span>
          <span class="text-gray-600 dark:text-gray-400 text-xs sm:text-sm">
            {{ sourceJobs.length }} {{ sourceJobs.length === 1 ? 'job' : 'jobs' }}
          </span>
        </div>

        <!-- Job Cards -->
        <div class="space-y-3">
          <JobCard
            v-for="job in sourceJobs"
            :key="job.id"
            :job="job"
          />
        </div>
      </div>
    </div>
  </div>
</template>
