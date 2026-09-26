/**
 * Single place to point the SPA at the backend. Angular's newer CLI dropped the old
 * environment.ts file-replacement setup — for this project's scope, one constant is enough;
 * swap it (or wire real build-time replacement) before a real deployment.
 */
export const API_BASE_URL = 'http://localhost:5080/api';
export const HUB_BASE_URL = 'http://localhost:5080/hubs/ride';
export const STRIPE_PUBLISHABLE_KEY = 'pk_test_replace_with_your_stripe_test_publishable_key';

/**
 * CARTO's free basemap tiles (carto.com/basemaps) — swapped in for raw OpenStreetMap tiles
 * because tile.openstreetmap.org actively blocks cloud/datacenter-originated traffic (see
 * docs/ROADMAP.md). This is a map-display key, not a secret: it's designed to sit in
 * client-side code and is restricted by CARTO on their end, same model as a Google Maps key.
 */
export const CARTO_API_KEY = 'cb1_3zcn_1_64892ba3af1b9c7fce8a3d07';
