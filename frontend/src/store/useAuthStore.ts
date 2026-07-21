import { create } from 'zustand';
import { persist } from 'zustand/middleware';

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
  user: User | null;
  theme: TenantTheme | null;
  
  // Actions
  login: (token: string, user: User, theme?: TenantTheme) => void;
  logout: () => void;
  setTheme: (theme: TenantTheme) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      theme: null,
      
      login: (token, user, theme) => {
        set({ token, user, theme });
        
        // Aplica o tema dinâmico no login
        if (theme?.primaryColor) {
          applyTenantTheme(theme.primaryColor);
        }
      },
      
      logout: () => {
        set({ token: null, user: null, theme: null });
        // Reseta o tema
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
      name: 'fleetos-auth-storage', // Key for localStorage
    }
  )
);

function applyTenantTheme(hexColor: string) {
  if (!/^#[0-9A-Fa-f]{6}$/.test(hexColor)) return;
  document.documentElement.style.setProperty('--brand-color', hexColor);
}
