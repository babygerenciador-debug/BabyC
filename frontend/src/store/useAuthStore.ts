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

function hexToHsl(hex: string): { h: number; s: number; l: number } | null {
  const match = hex.match(/^#([0-9A-Fa-f]{2})([0-9A-Fa-f]{2})([0-9A-Fa-f]{2})$/);
  if (!match) return null;
  let r = parseInt(match[1], 16) / 255;
  let g = parseInt(match[2], 16) / 255;
  let b = parseInt(match[3], 16) / 255;
  const max = Math.max(r, g, b), min = Math.min(r, g, b);
  let h = 0, s = 0, l = (max + min) / 2;
  if (max !== min) {
    const d = max - min;
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
    switch (max) {
      case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break;
      case g: h = ((b - r) / d + 2) / 6; break;
      case b: h = ((r - g) / d + 4) / 6; break;
    }
  }
  return { h: Math.round(h * 360), s: Math.round(s * 100), l: Math.round(l * 100) };
}

function applyTenantTheme(hexColor: string) {
  if (!/^#[0-9A-Fa-f]{6}$/.test(hexColor)) return;
  const root = document.documentElement;
  root.style.setProperty('--brand-color', hexColor);
  const hsl = hexToHsl(hexColor);
  if (hsl) {
    root.style.setProperty('--brand-h', String(hsl.h));
    root.style.setProperty('--brand-s', `${hsl.s}%`);
    root.style.setProperty('--brand-l', `${hsl.l}%`);
  }
}
