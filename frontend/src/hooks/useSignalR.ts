import { useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store/useAuthStore';
import { queryClient } from '../services/queryClient';

const HUB_URL = (() => {
  const envUrl = import.meta.env.VITE_API_URL;
  if (envUrl) {
    return envUrl.replace(/\/api\/v1\/?$/, '').replace(/\/$/, '') + '/hubs/fleet';
  }
  if (typeof window !== 'undefined') {
    const proto = window.location.protocol;
    const host = window.location.hostname;
    const port = window.location.port || (proto === 'https:' ? '443' : '80');
    return `${proto}//${host}:${port}/hubs/fleet`;
  }
  return '/hubs/fleet';
})();

let connection: signalR.HubConnection | null = null;
let connectionToken: string | null = null;
let connectionStarted = false;
let startPromise: Promise<void> | null = null;

function registerHandlers(conn: signalR.HubConnection) {
  // Dashboard: invalidate on any data change
  conn.on('DashboardUpdate', () => {
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  // Trips: all trip events
  conn.on('TripCreated', () => {
    queryClient.invalidateQueries({ queryKey: ['trips'] });
    queryClient.invalidateQueries({ queryKey: ['available-vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['my-active-trip'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  conn.on('TripUpdated', () => {
    queryClient.invalidateQueries({ queryKey: ['trips'] });
    queryClient.invalidateQueries({ queryKey: ['available-vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['my-active-trip'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  conn.on('TripVehicleSwapped', () => {
    queryClient.invalidateQueries({ queryKey: ['trips'] });
    queryClient.invalidateQueries({ queryKey: ['available-vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['my-active-trip'] });
  });

  // Vehicles
  conn.on('VehicleCreated', () => {
    queryClient.invalidateQueries({ queryKey: ['vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['available-vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  conn.on('VehicleUpdated', () => {
    queryClient.invalidateQueries({ queryKey: ['vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['available-vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  // Drivers
  conn.on('DriverCreated', () => {
    queryClient.invalidateQueries({ queryKey: ['drivers'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  conn.on('DriverUpdated', () => {
    queryClient.invalidateQueries({ queryKey: ['drivers'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  // Finance
  conn.on('TransactionCreated', () => {
    queryClient.invalidateQueries({ queryKey: ['transactions'] });
    queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  conn.on('TransactionUpdated', () => {
    queryClient.invalidateQueries({ queryKey: ['transactions'] });
    queryClient.invalidateQueries({ queryKey: ['cash-flow-summary'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  // Maintenance
  conn.on('MaintenanceCreated', () => {
    queryClient.invalidateQueries({ queryKey: ['maintenance'] });
    queryClient.invalidateQueries({ queryKey: ['vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  conn.on('MaintenanceUpdated', () => {
    queryClient.invalidateQueries({ queryKey: ['maintenance'] });
    queryClient.invalidateQueries({ queryKey: ['vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  // Fuel
  conn.on('FuelLogCreated', () => {
    queryClient.invalidateQueries({ queryKey: ['fuel-logs'] });
    queryClient.invalidateQueries({ queryKey: ['vehicles'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  // Inventory
  conn.on('StockUpdated', () => {
    queryClient.invalidateQueries({ queryKey: ['products'] });
    queryClient.invalidateQueries({ queryKey: ['stock-balance'] });
    queryClient.invalidateQueries({ queryKey: ['movements'] });
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  // Legacy / generic
  conn.on('ReceiveDashboardUpdate', () => {
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
  });

  conn.on('ReceiveNotification', () => {
    queryClient.invalidateQueries({ queryKey: ['notifications'] });
  });
}

function getOrCreateConnection(token: string): signalR.HubConnection {
  if (connection && connectionToken === token) {
    return connection;
  }

  if (connection) {
    connection.stop().catch(() => {});
  }

  connectionToken = token;
  connectionStarted = false;
  startPromise = null;

  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect({
      nextRetryDelayInMilliseconds: (retryContext) => {
        if (retryContext.elapsedMilliseconds < 30000) return 2000;
        if (retryContext.elapsedMilliseconds < 120000) return 5000;
        return 10000;
      }
    })
    .build();

  registerHandlers(connection);

  connection.onreconnecting(() => {
    console.log('SignalR reconectando...');
  });

  connection.onreconnected(() => {
    console.log('SignalR reconectado');
    queryClient.invalidateQueries({ queryKey: ['dashboardSummary'] });
    queryClient.invalidateQueries({ queryKey: ['trips'] });
  });

  connection.onclose(() => {
    connectionStarted = false;
  });

  return connection;
}

async function ensureConnected(token: string): Promise<void> {
  if (connectionStarted && connection?.state === signalR.HubConnectionState.Connected) {
    return;
  }

  const conn = getOrCreateConnection(token);

  if (conn.state === signalR.HubConnectionState.Disconnected) {
    if (!startPromise) {
      startPromise = conn.start().then(() => {
        connectionStarted = true;
        console.log('SignalR conectado:', HUB_URL);
      }).catch((err) => {
        connectionStarted = false;
        startPromise = null;
        console.error('SignalR erro:', err);
      });
    }
    await startPromise;
  }
}

export function useSignalR() {
  const token = useAuthStore((state) => state.token);

  useEffect(() => {
    if (!token) {
      if (connection) {
        connection.stop().catch(() => {});
        connection = null;
        connectionToken = null;
        connectionStarted = false;
        startPromise = null;
      }
      return;
    }

    ensureConnected(token);
  }, [token]);
}
