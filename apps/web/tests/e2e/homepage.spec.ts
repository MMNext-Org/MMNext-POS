import { test, expect } from '@playwright/test';

test('should navigate to homepage', async ({ page }) => {
  // Navigate to homepage
  await page.goto('/');
  
  // Verify page title
  await expect(page.title()).toBeTruthy();
  
  // Verify heading text
  await expect(page.locator('h1')).toHaveText('BREWFLOW SaaS Console');
});