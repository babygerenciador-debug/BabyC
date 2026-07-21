import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false, // Evita requisições desnecessárias
      retry: 1,                    // Tenta apenas mais 1 vez em caso de falha
      staleTime: 1000 * 60 * 5,    // 5 minutos até considerar cache stale
    },
  },
});
