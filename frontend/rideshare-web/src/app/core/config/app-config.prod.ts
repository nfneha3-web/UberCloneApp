/**
 * Production build values — swapped in for app-config.ts via angular.json's
 * "production" fileReplacements. Filled in once the Azure resources exist (see docs/DEPLOYMENT.md).
 */
export const API_BASE_URL = 'https://REPLACE-WITH-APP-SERVICE-NAME.azurewebsites.net/api';
export const HUB_BASE_URL = 'https://REPLACE-WITH-APP-SERVICE-NAME.azurewebsites.net/hubs/ride';
export const STRIPE_PUBLISHABLE_KEY = 'pk_test_replace_with_your_stripe_test_publishable_key';
