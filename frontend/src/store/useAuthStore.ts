import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  tenantId: string;
}

interface TenantTheme {
  primaryColor?: string;
  logoUrl?: string;
  companyName?: string;
}

interface AuthState {
  token: string | null;
  refreshToken: string | null;
  user: User | null;
  theme: TenantTheme | null;
  
  // Actions
  login: (token: string, refreshToken: string, user: User, theme?: TenantTheme) => void;
  setTokens: (token: string, refreshToken: string) => void;
  logout: () => void;
  setTheme: (theme: TenantTheme) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      refreshToken: null,
      user: null,
      theme: null,
      
      login: (token, refreshToken, user, theme) => {
        set({ token, refreshToken, user, theme });
        
        if (theme?.primaryColor) {
          applyTenantTheme(theme.primaryColor);
        }
      },

      setTokens: (token, refreshToken) => {
        set({ token, refreshToken });
      },
      
      logout: () => {
        set({ token: null, refreshToken: null, user: null, theme: null });
        document.documentElement.style.removeProperty('--brand-h');
        document.documentElement.style.removeProperty('--brand-s');
        document.documentElement.style.removeProperty('--brand-l');
      },

      setTheme: (theme) => {
        set({ theme });
        if (theme.primaryColor) {
          applyTenantTheme(theme.primaryColor);
        }
      }
    }),
    {
      name: 'fleetos-auth-storage',
      storage: createJSONStorage(() => sessionStorage),
    }
  )
);

function applyTenantTheme(hexColor: string) {
  if (!/^#[0-9A-Fa-f]{6}$/.test(hexColor)) return;
  document.documentElement.style.setProperty('--brand-color', hexColor);
}
