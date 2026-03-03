# Getting Started with Create React App and Fluent UI

This is a [Create React App](https://github.com/facebook/create-react-app) based repo that comes with Fluent UI pre-installed!

## Setup

Create a `.env` file within the base of the `reactd-fluent` folder with the following configuration:

- `VITE_API_BASE_URL` - Base URL for all api requests, (ex: `http://localhost:3100`)

> Note: The URL must include the schema, either `http://` or `https://`.

- `VITE_APPLICATIONINSIGHTS_CONNECTION_STRING` - Azure Application Insights connection string

## Available Scripts

In the project directory, you can run:

### `npm ci`

Installs local pre-requisites.

### `npm run dev`

Runs the app in development mode with Vite. Open [http://localhost:5173](http://localhost:5173) (or the port Vite reports) to view it in the browser.

### `npm test`

Runs the test suite once using [Vitest](https://vitest.dev/). Use for CI or a single run.

### `npm run test:watch`

Runs Vitest in watch mode. Tests re-run when you change files.

### `npm run test:ui`

Opens the [Vitest UI](https://vitest.dev/guide/ui.html) for browsing and running tests in the browser.

### `npm run test:coverage`

Runs Vitest with coverage (V8). Enforces 80% thresholds for statements, branches, functions, and lines. Report is written to `coverage/`.

**Testing stack:** Vitest (unit/component tests), React Testing Library and `@testing-library/jest-dom`, MSW for API mocking (`src/mocks/`), custom `render` from `src/test-utils/render.tsx` (theme + router), and test data factories in `src/test-utils/factories.ts`.

### `npm run build`

Builds the app for production to the `build` folder.
It correctly bundles React in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.
Your app is ready to be deployed!

See the section about [deployment](https://facebook.github.io/create-react-app/docs/deployment) for more information.

### `npm run eject`

**Note: this is a one-way operation. Once you `eject`, you can’t go back!**

If you aren’t satisfied with the build tool and configuration choices, you can `eject` at any time. This command will remove the single build dependency from your project.

Instead, it will copy all the configuration files and the transitive dependencies (Webpack, Babel, ESLint, etc) right into your project so you have full control over them. All of the commands except `eject` will still work, but they will point to the copied scripts so you can tweak them. At this point you’re on your own.

You don’t have to ever use `eject`. The curated feature set is suitable for small and middle deployments, and you shouldn’t feel obligated to use this feature. However we understand that this tool wouldn’t be useful if you couldn’t customize it when you are ready for it.

## Learn More

You can learn more in the [Create React App documentation](https://facebook.github.io/create-react-app/docs/getting-started).

To learn React, check out the [React documentation](https://reactjs.org/).

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
