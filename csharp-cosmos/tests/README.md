# ToDo Application Tests

The included [Playwright](https://playwright.dev/) tests run against the web app. The smoke test checks that the placeholder page is visible.

## Run Tests

The base URL is resolved in this order:

1. Value of `REACT_APP_WEB_BASE_URL` environment variable
2. Value of `REACT_APP_WEB_BASE_URL` from the default .azure environment
3. Default: `http://localhost:3000`

From the repo root:

```bash
cd csharp-cosmos/tests
npm i && npx playwright install firefox
npx playwright test
```

Or from this directory: `npm i && npx playwright install firefox` then `npx playwright test`.

**Browsers:** Tests use Firefox and WebKit only (Chromium is not used, e.g. when blocked by org policy). Use `--project=firefox` or `--project=webkit` to run a single browser.

Use `--headed` to open a browser while tests run.

## Debug Tests

Add the `--debug` flag to run with debugging enabled. You can find out more info here: https://playwright.dev/docs/next/test-cli#reference

```bash
npx playwright test --debug
```

More debugging references: https://playwright.dev/docs/debug and https://playwright.dev/docs/trace-viewer