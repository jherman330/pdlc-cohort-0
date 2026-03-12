#!/bin/sh
# Pre-commit hook that runs linting and tests on staged files

echo "Running pre-commit checks..."

FRONTEND_DIR="csharp-cosmos/src/web"

if [ ! -f "$FRONTEND_DIR/package.json" ]; then
  echo "ERROR: Frontend package.json not found at $FRONTEND_DIR"
  exit 1
fi

cd "$FRONTEND_DIR"

# Run lint-staged (ESLint + Prettier on staged files)
echo "Running linting checks..."
npx lint-staged

if [ $? -ne 0 ]; then
  echo "ERROR: Linting failed"
  exit 1
fi

# Run TypeScript type check
echo "Checking TypeScript types..."
npx tsc --noEmit

if [ $? -ne 0 ]; then
  echo "ERROR: TypeScript type checking failed"
  exit 1
fi

# Run tests
echo "Running tests..."
npm run test

if [ $? -ne 0 ]; then
  echo "ERROR: Tests failed"
  exit 1
fi

echo "All pre-commit checks passed."
cd - > /dev/null

exit 0
