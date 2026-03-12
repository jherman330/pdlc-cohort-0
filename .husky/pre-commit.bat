@echo off
setlocal enabledelayedexpansion

echo Running pre-commit checks...

set "FRONTEND_DIR=csharp-cosmos\src\web"

if not exist "%FRONTEND_DIR%\package.json" (
    echo ERROR: Frontend package.json not found at %FRONTEND_DIR%
    exit /b 1
)

cd /d "%FRONTEND_DIR%"

echo Formatting code with Prettier...
call npx prettier --write .
if !errorlevel! neq 0 (
    echo ERROR: Prettier formatting failed
    exit /b 1
)

echo Checking TypeScript types...
call npx tsc --noEmit
if !errorlevel! neq 0 (
    echo ERROR: TypeScript type checking failed
    exit /b 1
)

echo Running ESLint and Prettier check...
call npm run lint
if !errorlevel! neq 0 (
    echo ERROR: Linting failed - see errors above
    exit /b 1
)

echo Running tests...
call npm run test
if !errorlevel! neq 0 (
    echo ERROR: Tests failed
    exit /b 1
)

echo All pre-commit checks passed.
exit /b 0

echo "All pre-commit checks passed."
cd - > /dev/null

exit 0
