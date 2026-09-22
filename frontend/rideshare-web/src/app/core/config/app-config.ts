/**
 * Single place to point the SPA at the backend. Angular's newer CLI dropped the old
 * environment.ts file-replacement setup — for this project's scope, one constant is enough;
 * swap it (or wire real build-time replacement) before a real deployment.
 */
export const API_BASE_URL = 'http://localhost:5080/api';
export const HUB_BASE_URL = 'http://localhost:5080/hubs/ride';
export const STRIPE_PUBLISHABLE_KEY = 'pk_test_replace_with_your_stripe_test_publishable_key';
