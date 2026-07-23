import { defineStore } from 'pinia'
import type {
  ActiveProgrammeDto,
  ProgrammeTemplateDto,
  AdoptProgrammeCommand,
  LogProgrammeSessionCommand,
} from '~/lib/web-api-client'

export const useProgrammeStore = defineStore('programme', {
  state: () => ({
    activeProgramme: null as ActiveProgrammeDto | null,
    templates: [] as ProgrammeTemplateDto[],
    loading: false,
    error: null as string | null,
  }),

  getters: {
    hasActiveProgramme: (state) => state.activeProgramme !== null,
    nextSession: (state) => state.activeProgramme?.nextSession ?? null,
  },

  actions: {
    async fetchActiveProgramme() {
      this.loading = true
      this.error = null
      try {
        const client = useProgrammesClient()
        const result = await client.getActiveProgramme()
        // The generated client returns an empty ActiveProgrammeDto (not null) when the
        // API responds with a null body, so normalize on a real field to detect "no programme".
        this.activeProgramme = result?.id !== undefined ? result : null
      } catch (e: any) {
        this.error = e?.message ?? 'Failed to load programme'
      } finally {
        this.loading = false
      }
    },

    async fetchTemplates() {
      const client = useProgrammesClient()
      this.templates = await client.getProgrammeTemplates()
    },

    async adoptProgramme(command: AdoptProgrammeCommand) {
      const client = useProgrammesClient()
      const id = await client.adoptProgramme(command)
      await this.fetchActiveProgramme()
      return id
    },

    async logProgrammeSession(programmeId: number, command: LogProgrammeSessionCommand) {
      const client = useProgrammesClient()
      const workoutId = await client.logProgrammeSession(programmeId, command)
      await this.fetchActiveProgramme()
      return workoutId
    },
  },
})
