import { randomUUID } from 'node:crypto';
import { test } from '@playwright/test';
import { RegisterPage } from '../pages/register.page';

const uniqueEmail = () => `testuser-${randomUUID().replace(/-/g, '')}@example.com`;

test('user can register with a valid email and password', async ({ page }) => {
  const registerPage = new RegisterPage(page);
  await registerPage.goto();

  await registerPage.setEmail(uniqueEmail());
  await registerPage.setPassword('ValidPass1!');
  await registerPage.clickRegister();

  await registerPage.assertRedirectedToLogin();
});

test('user cannot register with a password that is too short', async ({ page }) => {
  const registerPage = new RegisterPage(page);
  await registerPage.goto();

  await registerPage.setEmail(uniqueEmail());
  await registerPage.setPassword('abc');
  await registerPage.clickRegister();

  await registerPage.assertPasswordHelperTextVisible();
  await registerPage.assertStillOnRegisterPage();
});
