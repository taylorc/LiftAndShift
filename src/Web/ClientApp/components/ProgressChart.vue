<template>
  <div style="width: 100%; overflow-x: auto;">
    <svg :viewBox="`0 0 ${width} ${height}`" style="width: 100%; min-width: 300px;" xmlns="http://www.w3.org/2000/svg">
      <!-- Grid lines -->
      <g v-for="i in 5" :key="i">
        <line
          :x1="padding"
          :y1="padding + (chartHeight / 4) * (i - 1)"
          :x2="width - padding"
          :y2="padding + (chartHeight / 4) * (i - 1)"
          stroke="var(--pico-muted-border-color)"
          stroke-width="1"
        />
        <text
          :x="padding - 4"
          :y="padding + (chartHeight / 4) * (i - 1) + 4"
          text-anchor="end"
          font-size="11"
          fill="var(--pico-muted-color)"
        >{{ formatY(maxVal - (maxVal - minVal) / 4 * (i - 1)) }}</text>
      </g>

      <!-- Line path -->
      <polyline
        v-if="points.length > 1"
        :points="svgPoints"
        fill="none"
        stroke="var(--pico-primary)"
        stroke-width="2"
        stroke-linejoin="round"
      />

      <!-- Data points -->
      <g v-for="(pt, i) in points" :key="i">
        <circle
          :cx="pt.x"
          :cy="pt.y"
          r="4"
          fill="var(--pico-primary)"
        />
        <title>{{ formatDate(data[i].date) }}: {{ data[i][yField] }}kg</title>
      </g>

      <!-- X axis labels -->
      <g v-for="(pt, i) in labelPoints" :key="'lbl' + i">
        <text
          :x="pt.x"
          :y="height - 4"
          text-anchor="middle"
          font-size="10"
          fill="var(--pico-muted-color)"
        >{{ pt.label }}</text>
      </g>
    </svg>
  </div>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
  data: { date: string; [key: string]: any }[]
  yField: string
  width?: number
  height?: number
}>(), {
  width: 700,
  height: 300,
})

const padding = 50
const chartWidth = computed(() => props.width - padding * 2)
const chartHeight = computed(() => props.height - padding * 2)

const values = computed(() => props.data.map(d => Number(d[props.yField]) || 0))
const minVal = computed(() => Math.max(0, Math.min(...values.value) * 0.9))
const maxVal = computed(() => Math.max(...values.value) * 1.05 || 100)

const points = computed(() => {
  if (!props.data.length) return []
  const n = props.data.length
  return props.data.map((_, i) => ({
    x: padding + (i / Math.max(n - 1, 1)) * chartWidth.value,
    y: padding + (1 - (values.value[i] - minVal.value) / (maxVal.value - minVal.value)) * chartHeight.value,
  }))
})

const svgPoints = computed(() =>
  points.value.map(p => `${p.x},${p.y}`).join(' ')
)

const labelPoints = computed(() => {
  const n = props.data.length
  const step = Math.max(1, Math.ceil(n / 6))
  return props.data
    .filter((_, i) => i % step === 0 || i === n - 1)
    .map((d, idx) => {
      const i = idx * step
      const x = padding + (Math.min(i, n - 1) / Math.max(n - 1, 1)) * chartWidth.value
      return { x, label: formatDate(d.date) }
    })
})

function formatDate(dateStr: string) {
  const d = new Date(dateStr)
  return `${d.getMonth() + 1}/${d.getDate()}`
}

function formatY(v: number) {
  return Math.round(v)
}
</script>
