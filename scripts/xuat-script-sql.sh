#!/usr/bin/env bash
# Sinh lại ShopBanHang/Data/Scripts/ShopBanHang.sql từ toàn bộ migration (macOS / Linux).
#     bash scripts/xuat-script-sql.sh
# Xem giải thích trong xuat-script-sql.ps1.
set -euo pipefail
cd "$(dirname "$0")/.."

echo "1/3 Cài dotnet-ef theo .config/dotnet-tools.json ..."
dotnet tool restore

echo "2/3 Build project ..."
dotnet build ShopBanHang/ShopBanHang.csproj -c Debug --nologo -v q

echo "3/3 Xuất script SQL ..."
dotnet ef migrations script --idempotent \
    --project ShopBanHang/ShopBanHang.csproj \
    --output ShopBanHang/Data/Scripts/ShopBanHang.sql \
    --no-build

echo "Xong: ShopBanHang/Data/Scripts/ShopBanHang.sql"
