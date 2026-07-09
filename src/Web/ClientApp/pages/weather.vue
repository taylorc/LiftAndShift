<script setup lang="ts">
definePageMeta({ middleware: 'protected' })

const client = useWeatherForecastsClient()

const { data: forecasts, error, pending } = await useAsyncData(
  'weather',
  () => client.getWeatherForecasts(),
)

function formatDate(date: Date | string | undefined) {
  if (!date) return ''
  return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}
</script>

<template>
  <div>
    <h1>Weather</h1>
    <p>This component demonstrates fetching data from the server.</p>
    <span v-if="pending" aria-busy="true">Fetching your weather forecast…</span>
    <p v-else-if="error" class="error">Unable to load weather forecasts. Please try again later.</p>
    <table v-else>
      <thead>
        <tr>
          <th>Date</th>
          <th>Temp. (C)</th>
          <th>Temp. (F)</th>
          <th>Summary</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="forecast in forecasts" :key="String(forecast.date)">
          <td>{{ formatDate(forecast.date) }}</td>
          <td>{{ forecast.temperatureC }}</td>
          <td>{{ forecast.temperatureF }}</td>
          <td>{{ forecast.summary }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
