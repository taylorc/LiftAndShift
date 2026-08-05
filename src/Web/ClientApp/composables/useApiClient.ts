import {
  UsersClient,
  OnboardingClient,
  TodoListsClient,
  TodoItemsClient,
  WeatherForecastsClient,
  ExercisesClient,
  WorkoutsClient,
  ProgrammesClient,
  CalculatorsClient,
  BodyMetricsClient,
  DashboardClient,
} from '~/lib/web-api-client'

function buildHttp(cookieHeader?: string) {
  return {
    fetch(url: RequestInfo, init?: RequestInit): Promise<Response> {
      const headers = new Headers(init?.headers)
      if (import.meta.server && cookieHeader) {
        headers.set('Cookie', cookieHeader)
      }
      return globalThis.fetch(url, {
        ...init,
        headers,
        credentials: import.meta.client ? 'include' : undefined,
      })
    },
  }
}

function getBaseUrl(): string {
  const config = useRuntimeConfig()
  return import.meta.server ? (config.apiBaseUrl as string) : ''
}

function makeClient<T>(Ctor: new (base: string, http: any) => T): T {
  const cookieHeader = import.meta.server
    ? useRequestHeaders(['cookie']).cookie
    : undefined
  return new Ctor(getBaseUrl(), buildHttp(cookieHeader))
}

export const useUsersClient = () => makeClient(UsersClient)
export const useOnboardingClient = () => makeClient(OnboardingClient)
export const useTodoListsClient = () => makeClient(TodoListsClient)
export const useTodoItemsClient = () => makeClient(TodoItemsClient)
export const useWeatherForecastsClient = () => makeClient(WeatherForecastsClient)
export const useExercisesClient = () => makeClient(ExercisesClient)
export const useWorkoutsClient = () => makeClient(WorkoutsClient)
export const useProgrammesClient = () => makeClient(ProgrammesClient)
export const useCalculatorsClient = () => makeClient(CalculatorsClient)
export const useBodyMetricsClient = () => makeClient(BodyMetricsClient)
export const useDashboardClient = () => makeClient(DashboardClient)
