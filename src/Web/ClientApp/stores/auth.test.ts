import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'
import { setActivePinia, createPinia } from 'pinia'
import { LoginRequest, RegisterRequest, type InfoResponse } from '~/lib/web-api-client'


const { infoGET, login, register, logout } = vi.hoisted(() => ({
  infoGET: vi.fn(),
  login: vi.fn(),
  register: vi.fn(),
  logout: vi.fn()
}))

mockNuxtImport('useUsersClient', () => {
  return () => ({ infoGET, login, register, logout })
})


describe('useAuth', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('on initialisation isAuthenticated is set to true', async () => {
    const store = useAuthStore();
    infoGET.mockResolvedValueOnce({email: "admin@localhost.com", isEmailConfirmed: true})
    await store.initAuth();

    expect(store.isAuthenticated).toBe(true);
  })

  it('on successful login isAuthenticated is set to true', async () => {
    const store = useAuthStore();
    /*tokenType?: string | undefined;
    accessToken!: string;
    expiresIn!: number;
    refreshToken!: string;*/
    login.mockResolvedValueOnce({tokenType: "jwt", accessToken: "XXXXXX", expiresIn: 57684903, refreshToken: "YYYYYY"})
    await store.login("admin@localhost.com", "p@ssw0rd");

    expect(store.isAuthenticated).toBe(true);
  })

  it('login is called with the correct parameters', async () => {
    const store = useAuthStore();
    login.mockResolvedValueOnce({tokenType: "jwt", accessToken: "XXXXXX", expiresIn: 57684903, refreshToken: "YYYYYY"})

    await store.login("admin@localhost.com", "p@ssw0rd");

    expect(login).toBeCalledWith(true, undefined, new LoginRequest({ email: "admin@localhost.com", password:"p@ssw0rd"}));

  })

   it('login is called with invalid email an error is thrown', async () => {
    const store = useAuthStore();

    await expect(store.login("admin", "p@ssw0rd")).rejects.toThrowError(
      "The email provided is invalid. Please try again"
    );
    expect(login).not.toHaveBeenCalled();
  })

  it('when called register calls the api method register', async () => {
    const store = useAuthStore();
    await store.register("admin@localhost.com","p2SSWORDS");

    expect(register).toHaveBeenCalledOnce();
  })

  it('register is called with the correct parameters', async () => {
    const store = useAuthStore();
    await store.register("admin@localhost.com","p2SSWORDS");

    expect(register).toBeCalledWith(new RegisterRequest({email: "admin@localhost.com", password: "p2SSWORDS"}));
  })

  it('register is called with invalid email an error is thrown', async () => {
    const store = useAuthStore();

    await expect(store.register("admin", "p@ssw0rd")).rejects.toThrowError(
      "The email provided is invalid. Please try again"
    );
    expect(register).not.toHaveBeenCalled();
  })


  it('on successful logout isAuthenticated is set to false and isonboarded set to false', async () => {
    const store = useAuthStore();
    /*tokenType?: string | undefined;
    accessToken!: string;
    expiresIn!: number;
    refreshToken!: string;*/
    await store.logout();

    expect(store.isAuthenticated).toBe(false);
    expect(store.isOnboarded).toBe(false);
  })
})
