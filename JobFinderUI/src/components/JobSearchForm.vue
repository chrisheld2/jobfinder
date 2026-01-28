<script setup>
import { ref } from 'vue'

defineProps({
  loading: Boolean
})

const emit = defineEmits(['search', 'source-change'])

const source = ref('all')

const handleSearch = () => {
  emit('search', source.value)
}

const handleSourceChange = () => {
  emit('source-change', source.value)
}
</script>

<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4 sm:p-6 transition-colors duration-200">
    <h2 class="text-lg sm:text-xl font-semibold text-gray-900 dark:text-white mb-4">
      Search Jobs
    </h2>

    <div class="space-y-4">
      <!-- Job Source Selection -->
      <div>
        <label for="source" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Job Source
        </label>
        <div class="grid grid-cols-3 gap-2 sm:gap-3">
          <label class="flex items-center cursor-pointer">
            <input
              type="radio"
              value="all"
              v-model="source"
              @change="handleSourceChange"
              class="w-4 h-4 text-indigo-600 dark:text-indigo-400"
            />
            <span class="ml-2 text-sm sm:text-base text-gray-700 dark:text-gray-300">All</span>
          </label>
          <label class="flex items-center cursor-pointer">
            <input
              type="radio"
              value="indeed"
              v-model="source"
              @change="handleSourceChange"
              class="w-4 h-4 text-indigo-600 dark:text-indigo-400"
            />
            <span class="ml-2 text-sm sm:text-base text-gray-700 dark:text-gray-300">Indeed</span>
          </label>
          <label class="flex items-center cursor-pointer">
            <input
              type="radio"
              value="monster"
              v-model="source"
              @change="handleSourceChange"
              class="w-4 h-4 text-indigo-600 dark:text-indigo-400"
            />
            <span class="ml-2 text-sm sm:text-base text-gray-700 dark:text-gray-300">Monster</span>
          </label>
        </div>
      </div>

      <!-- Search Button -->
      <button
        @click="handleSearch"
        :disabled="loading"
        class="w-full bg-indigo-600 hover:bg-indigo-700 dark:bg-indigo-500 dark:hover:bg-indigo-600 disabled:bg-indigo-400 dark:disabled:bg-indigo-700 text-white font-semibold py-3 px-4 rounded-lg transition-colors duration-200"
      >
        <span v-if="loading" class="inline-flex items-center">
          <span class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></span>
          Searching...
        </span>
        <span v-else>🔍 Search Jobs</span>
      </button>
    </div>

    <!-- Info Box -->
    <div class="mt-4 bg-blue-50 dark:bg-blue-900/20 border-l-4 border-blue-500 dark:border-blue-400 p-3 sm:p-4">
      <p class="text-xs sm:text-sm text-blue-800 dark:text-blue-300">
        Searching for Full-Stack Developer positions with C#, Azure, and security clearance requirements.
      </p>
    </div>
  </div>
</template>
