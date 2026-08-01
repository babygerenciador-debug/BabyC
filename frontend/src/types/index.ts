// ============================================================
// FleetOS Frontend — Core TypeScript Types
// ============================================================

// ─── Shared Types ────────────────────────────────────────────────────

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface ApiError {
  code: string;
  description: string;
  traceId?: string;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PageFilter {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

// ─── Auth Types ───────────────────────────────────────────────────────

export type UserRole = 'SystemAdmin' | 'TenantAdmin' | 'Manager' | 'Driver';

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  tenantId: string;
  organizationId: string;
  businessUnitId: string;
  theme: string;
  language: string;
  // Driver specific
  isDriverAccount?: boolean;
  cpfLast4?: string;
}

export interface LoginRequest {
  identifier: string;   // email (admin/manager) or CPF (driver)
  password: string;
  tenantSlug?: string;  // required for CPF login
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: AuthUser;
  fuelAlert?: FuelAlert[];  // Driver login: pending fuel alerts
}

export interface FuelAlert {
  vehicleId: string;
  vehiclePlate: string;
  vehicleNickname: string;
  daysSinceLastFuel: number;
  alertMode: 'SinceLastFuel' | 'FixedCycle';
}

// ─── Tenant / Org / BU ───────────────────────────────────────────────

export interface Tenant {
  id: string;
  name: string;
  slug: string;
  status: 'Active' | 'Suspended' | 'Cancelled';
  plan: 'Trial' | 'Starter' | 'Professional' | 'Enterprise';
  logoUrl?: string;
  primaryColor?: string;
  timeZone: string;
  language: string;
  createdAt: string;
}

export interface Organization {
  id: string;
  tenantId: string;
  name: string;
  cnpj?: string;
  status: 'Active' | 'Disabled';
  phone?: string;
  email?: string;
  city?: string;
  state?: string;
  logoUrl?: string;
}

export interface BusinessUnit {
  id: string;
  tenantId: string;
  organizationId: string;
  name: string;
  code: string;
  status: 'Active' | 'Suspended' | 'Archived';
  isHeadOffice: boolean;
  city?: string;
  state?: string;
  phone?: string;
  email?: string;
}

// ─── Users ───────────────────────────────────────────────────────────

export interface User {
  id: string;
  tenantId: string;
  name: string;
  emailAddress: string;
  role: UserRole;
  status: 'Active' | 'Locked' | 'Disabled' | 'Archived';
  lastLoginAt?: string;
  createdAt: string;
  isDriverAccount: boolean;
  cpfLast4?: string;
}

// ─── Drivers ─────────────────────────────────────────────────────────

export type DriverStatus = 'Active' | 'Blocked' | 'Dismissed';

export interface Driver {
  id: string;
  tenantId: string;
  businessUnitId: string;
  userId: string;
  name: string;
  cpfLast4: string;
  status: DriverStatus;
  // CNH
  cnhNumber?: string;
  cnhCategory?: string;
  cnhExpiry?: string;
  isCnhExpired: boolean;
  // Personal
  phone?: string;
  email?: string;
  photoUrl?: string;
  // Assignment
  assignedVehicleId?: string;
  assignedVehicleNickname?: string;
  isAvailable: boolean;
  createdAt: string;
}

export interface CreateDriverRequest {
  name: string;
  cpf: string;
  phone?: string;
  email?: string;
  businessUnitId: string;
  cnhNumber?: string;
  cnhCategory?: string;
  cnhExpiry?: string;
}

// ─── Vehicles ────────────────────────────────────────────────────────

export type VehicleStatus = 'Active' | 'UnderMaintenance' | 'Inactive';
export type FuelAlertMode = 'SinceLastFuel' | 'FixedCycle';

export interface Vehicle {
  id: string;
  tenantId: string;
  businessUnitId: string;
  nickname: string;
  plate: string;
  model: string;
  brand?: string;
  year?: number;
  capacity: number;
  color?: string;
  photoUrl?: string;
  status: VehicleStatus;
  // Documents
  renavam?: string;
  chassis?: string;
  anttExpiry?: string;
  artespExpiry?: string;
  insuranceExpiry?: string;
  licensingExpiry?: string;
  // Fuel
  lastFuelAt?: string;
  fuelAlertDays?: number;
  fuelAlertMode?: FuelAlertMode;
  currentOdometerKm?: number;
  isAvailableForTrip: boolean;
  createdAt: string;
}

// ─── Trips ───────────────────────────────────────────────────────────

export type TripStatus = 'Scheduled' | 'InProgress' | 'Completed' | 'Cancelled';

export interface Trip {
  id: string;
  tenantId: string;
  businessUnitId: string;
  status: TripStatus;
  driverId: string;
  driverName: string;
  vehicleId: string;
  vehicleNickname: string;
  vehiclePlate: string;
  origin: string;
  destination: string;
  scheduledDepartAt: string;
  scheduledArriveAt?: string;
  departedAt?: string;
  arrivedAt?: string;
  purpose?: string;
  notes?: string;
  passengerCount?: number;
  createdAt: string;
}

// ─── Fuel Records ─────────────────────────────────────────────────────

export interface FuelRecord {
  id: string;
  vehicleId: string;
  vehicleNickname: string;
  vehiclePlate: string;
  driverId: string;
  driverName: string;
  date: string;
  liters: number;
  totalCost: number;
  pricePerLiter?: number;
  odometerKm?: number;
  kmDriven?: number;    // calculated: current - previous odometer
  fuelStation?: string;
  purpose?: string;
  receiptUrl?: string;
  createdAt: string;
}

// ─── Dashboard KPIs ───────────────────────────────────────────────────

export interface DashboardStats {
  totalVehicles: number;
  activeVehicles: number;
  totalDrivers: number;
  activeDrivers: number;
  scheduledTrips: number;
  pendingObservations: number;
  monthlyFuelCost: number;
  monthlyFuelLiters: number;
}

export interface VehicleFuelStatus {
  vehicleId: string;
  vehicleNickname: string;
  vehiclePlate: string;
  lastFuelAt?: string;
  daysSinceLastFuel?: number;
  alertMode: FuelAlertMode;
  alertThresholdDays: number;
  status: 'ok' | 'warning' | 'critical';
}

// ─── Observations ─────────────────────────────────────────────────────

export type ObservationUrgency = 'low' | 'normal' | 'high' | 'critical';
export type ObservationStatus = 'Pending' | 'InReview' | 'Resolved';

export interface Observation {
  id: string;
  tenantId: string;
  driverId: string;
  driverName: string;
  vehicleId?: string;
  vehicleNickname?: string;
  title: string;
  description: string;
  urgency: ObservationUrgency;
  status: ObservationStatus;
  photoUrl?: string;
  resolvedAt?: string;
  resolvedBy?: string;
  adminNotes?: string;
  createdAt: string;
}

// ─── Checklist ────────────────────────────────────────────────────────

export interface ChecklistTemplate {
  id: string;
  tenantId: string;
  name: string;
  isDefault: boolean;
  items: ChecklistTemplateItem[];
  createdAt: string;
}

export interface ChecklistTemplateItem {
  id: string;
  order: number;
  label: string;
  required: boolean;
  type: 'checkbox' | 'text' | 'number' | 'photo';
}

export interface ChecklistResponse {
  id: string;
  templateId: string;
  driverId: string;
  vehicleId?: string;
  tripId?: string;
  date: string;
  answers: ChecklistAnswer[];
  completedAt?: string;
}

export interface ChecklistAnswer {
  itemId: string;
  label: string;
  value: string | boolean | number;
  photoUrl?: string;
}

// ─── Settings ─────────────────────────────────────────────────────────

export interface TenantSettings {
  // Driver auth
  driverDefaultPasswordSet: boolean;    // Don't expose the password itself
  // Fuel alerts
  fuelAlertMode: FuelAlertMode;
  fuelAlertDays: number;
  // Branding
  primaryColor?: string;
  logoUrl?: string;
  // Localization
  timeZone: string;
  language: string;
}
