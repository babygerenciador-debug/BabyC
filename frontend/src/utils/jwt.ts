/**
 * JWT Utilities - Client-Side Validation (UX Optimization Only)
 * 
 * SECURITY NOTE: These functions are for UX only (preventing UI flash).
 * The backend validates JWT tokens on every protected request.
 * Client-side validation can be bypassed - it's NOT a security control.
 * 
 * JWT tokens are base64-encoded by design - anyone can decode them.
 * Security relies on token signature (which cannot be forged) and backend validation.
 */

/**
 * Check if JWT token is expired (client-side UX optimization)
 * 
 * @param token - JWT token string
 * @returns true if token is expired or invalid
 */
export function isTokenExpired(token: string): boolean {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return true;
    
    const payload = JSON.parse(atob(parts[1]));
    if (!payload.exp) return true;
    
    return payload.exp * 1000 < Date.now();
  } catch {
    return true;
  }
}
