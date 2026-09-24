<script setup lang="ts">
const identity = useIdentityStore()
</script>

<template>
  <div class="min-h-screen bg-paper text-ink font-body">
    <NuxtRouteAnnouncer />
    <header class="flex items-center justify-between border-b-2 border-ink px-6 py-4 sm:px-10">
      <div class="flex items-center gap-8">
        <NuxtLink to="/" class="font-display text-3xl font-black tracking-tight text-iron">
          LiftAndShift
        </NuxtLink>
        <template v-if="identity.familyMemberId">
          <NuxtLink to="/log" class="text-sm font-semibold text-iron transition-colors hover:text-iron-dark">
            Log Workout
          </NuxtLink>
          <NuxtLink to="/programme" class="text-sm font-medium text-ink/70 transition-colors hover:text-iron">
            Your Programme
          </NuxtLink>
          <NuxtLink to="/work-weights" class="text-sm font-medium text-ink/70 transition-colors hover:text-iron">
            Work Weights
          </NuxtLink>
        </template>
      </div>

      <div v-if="identity.name" class="flex items-center gap-3 text-sm">
        <span class="text-ink/70">Training as <strong class="font-semibold text-ink">{{ identity.name }}</strong></span>
        <button
          type="button"
          class="border border-chalk px-3 py-1 text-ink/70 transition-colors hover:border-iron hover:text-iron"
          @click="identity.clear()"
        >
          Not you?
        </button>
      </div>
    </header>
    <main class="px-6 py-8 sm:px-10">
      <NuxtPage />
    </main>
  </div>
</template>
