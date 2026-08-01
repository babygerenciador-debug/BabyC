/**
 * JWT Validation Utilities
 * 
 * IMPORTANT: These utilities are for UX optimization only (preventing UI flash before redirect).
 * Security-critical validation MUST happen on the backend.
 * 
 * Client-side JWT validation is NOT secure - tokens can be manipulated.
 * The backend validates tokens on every protected request.
 */

export interface JwtPayload {
  exp: number;
  iat?: number;
  sub?: string;
  email?: string;
  role?: string;
  tenantId?: string;
  [key: string]: unknown;
}

/**
 * Decode JWT payload without verification (for client-side UX only)
 * 
 * @param token - JWT token string
 * @returns Decoded payload or null if invalid
 */
export function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    
    const payload = parts[1];
    const decoded = atob(payload);
    return JSON.parse(decoded);
  } catch {
    return null;
  }
}

/**
 * Check if JWT token is expired (client-side check for UX only)
 * 
 * WARNING: This is NOT a security check. The backend must validate token expiration.
 * This function is used to prevent showing UI briefly before redirecting to login.
 * 
 * @param token - JWT token string
 * @returns true if token is expired or invalid
 */
export function isTokenExpired(token: string): boolean {
  const payload = decodeJwtPayload(token);
  if (!payload || !payload.exp) return true;
  
  // exp is in seconds, Date.now() is in milliseconds
  return payload.exp * 1000 < Date.now();
}

/**
 * Get time until token expires (in seconds)
 * 
 * @param token - JWT token string
 * @returns seconds until expiration, or 0 if expired/invalid
 */
export function getTimeUntilExpiry(token: string): number {
  const payload = decodeJwtPayload(token);
  if (!payload || !payload.exp) return 0;
  
  const expiresAt = payload.exp * 1000;
  const now = Date.now();
  const remaining = Math.max(0, expiresAt - now);
  
  return Math.floor(remaining / 1000);
}

/**
 * Check if token will expire soon (within threshold)
 * 
 * @param token - JWT token string
 * @param thresholdSeconds - threshold in seconds (default: 300 = 5 minutes)
 * @returns true if token will expire within threshold
 */
export function willExpireSoon(token: string, thresholdSeconds = 300): boolean {
  const timeUntilExpiry = getTimeUntilExpiry(token);
  return timeUntilExpiry > 0 && timeUntilExpiry < thresholdSeconds;
}
