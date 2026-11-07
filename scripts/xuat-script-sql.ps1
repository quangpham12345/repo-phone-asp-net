# =====================================================================
# Sinh lại ShopBanHang/Data/Scripts/ShopBanHang.sql từ toàn bộ migration.
#
# Chạy trong PowerShell (Windows) tại thư mục gốc repo:
#     powershell -ExecutionPolicy Bypass -File scripts\xuat-script-sql.ps1
#
# Script sinh ở chế độ --idempotent: mỗi migration bọc trong
# IF NOT EXISTS (... __EFMigrationsHistory ...), nên chạy trên CSDL trống
# hay CSDL đã có một phần đều được, chạy nhiều lần không lỗi.
#
# PHẢI chạy lại mỗi khi thêm migration mới (Add-Migration), rồi commit file .sql.
# =====================================================================
$ErrorActionPreference = "Stop"
$goc = Split-Path -Parent $PSScriptRoot
Set-Location $goc

Write-Host "1/3 Cài dotnet-ef theo phiên bản ghi trong .config/dotnet-tools.json ..."
dotnet tool restore

Write-Host "2/3 Build project ..."
dotnet build ShopBanHang/ShopBanHang.csproj -c Debug --nologo -v q

Write-Host "3/3 Xuất script SQL ..."
dotnet ef migrations script --idempotent `
    --project ShopBanHang/ShopBanHang.csproj `
    --output ShopBanHang/Data/Scripts/ShopBanHang.sql `
    --no-build

Write-Host "Xong: ShopBanHang/Data/Scripts/ShopBanHang.sql"
