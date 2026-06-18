import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { UsersClient, LoginRequest, RegisterRequest, OnboardingClient } from '../../web-api-client';

const AuthContext = createContext(null);

const usersClient = new UsersClient();
const onboardingClient = new OnboardingClient();

export function AuthProvider({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isOnboarded, setIsOnboarded] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  const fetchOnboardingStatus = useCallback(() =>
    onboardingClient.getOnboarding()
      .then(data => setIsOnboarded(data.isOnboarded ?? false))
      .catch(() => setIsOnboarded(false)),
    []
  );

  useEffect(() => {
    usersClient.infoGET()
      .then(() => {
        setIsAuthenticated(true);
        return fetchOnboardingStatus();
      })
      .catch(() => {
        setIsAuthenticated(false);
        setIsOnboarded(false);
      })
      .finally(() => setIsLoading(false));
  }, [fetchOnboardingStatus]);

  const login = (email, password) =>
    usersClient.login(true, undefined, new LoginRequest({ email, password }))
      .then(() => {
        setIsAuthenticated(true);
        return fetchOnboardingStatus();
      });

  const register = (email, password) =>
    usersClient.register(new RegisterRequest({ email, password }));

  const logout = () =>
    usersClient.logout({})
      .then(() => {
        setIsAuthenticated(false);
        setIsOnboarded(false);
      });

  return (
    <AuthContext.Provider value={{
      isAuthenticated,
      isOnboarded,
      isLoading,
      login,
      register,
      logout,
      refreshOnboardingStatus: fetchOnboardingStatus
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
