import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

type ThemeMode = 'light' | 'dark';

interface ThemeState {
  mode: ThemeMode;
  toggle: () => void;
}

function getSystemTheme(): ThemeMode {
  if (typeof window === 'undefined') return 'light';
  return window.matchMedia('(prefers-color-scheme:dark)').matches ? 'dark' : 'light';
}

function applyTheme(mode: ThemeMode) {
  document.documentElement.setAttribute('data-theme', mode);
  document.documentElement.style.colorScheme = mode;
  const meta = document.querySelector('meta[name="theme-color"]');
  if (meta) {
    meta.setAttribute('content', mode === 'dark' ? '#0b1120' : '#f1f5f9');
  }
}

const systemTheme = getSystemTheme();

export const useThemeStore = create<ThemeState>()(
  persist(
    (set, get) => ({
      mode: systemTheme,
      toggle: () => {
        const next = get().mode === 'light' ? 'dark' : 'light';
        applyTheme(next);
        set({ mode: next });
      },
    }),
    {
      name: 'fleetos-theme',
      storage: createJSONStorage(() => localStorage),
      onRehydrateStorage: () => (state) => {
        if (state?.mode) applyTheme(state.mode);
      },
    }
  )
);
