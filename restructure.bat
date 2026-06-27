@echo off
echo Rearranging project into frontend, backend, database structure...

REM Create new directories
if not exist "src\frontend" mkdir "src\frontend"
if not exist "src\backend" mkdir "src\backend"
if not exist "src\database" mkdir "src\database"

REM Move frontend
if exist "src\SchoolErp.Frontend" (
    move "src\SchoolErp.Frontend" "src\frontend\SchoolErp.Frontend"
)

REM Move backend projects
if exist "src\SchoolErp.WebAPI" (
    move "src\SchoolErp.WebAPI" "src\backend\SchoolErp.WebAPI"
)
if exist "src\SchoolErp.Application" (
    move "src\SchoolErp.Application" "src\backend\SchoolErp.Application"
)
if exist "src\SchoolErp.Infrastructure" (
    move "src\SchoolErp.Infrastructure" "src\backend\SchoolErp.Infrastructure"
)
if exist "src\SchoolErp.Domain" (
    move "src\SchoolErp.Domain" "src\backend\SchoolErp.Domain"
)

REM Move database scripts
if exist "cleanup_sample_data.sql" (
    move "cleanup_sample_data.sql" "src\database\cleanup_sample_data.sql"
)
if exist "delete_demo_user.sql" (
    move "delete_demo_user.sql" "src\database\delete_demo_user.sql"
)

echo.
echo Done! Please update your .sln and .csproj files with the new paths.
pause
