using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopBanHang.Migrations
{
    /// <inheritdoc />
    public partial class ThemGioHangVaThanhToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== Orders: tiền hàng, giảm giá, mã giảm giá, trạng thái thanh toán, huỷ =====

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Orders",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Orders",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CouponId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Orders",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            // ===== OrderDetails: biến thể đã mua + chép tên lúc mua =====

            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "OrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantName",
                table: "OrderDetails",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            // ===== Dữ liệu cũ: đơn trước đây chưa có tiền hàng và trạng thái thanh toán =====
            // Bọc trong EXEC để script idempotent (migrations script --idempotent) không báo
            // lỗi "Invalid column name" khi SQL Server biên dịch trước cả khối IF.
            // Đơn cũ không dùng mã nên tiền hàng = tổng tiền.
            migrationBuilder.Sql("EXEC(N'UPDATE [Orders] SET [Subtotal] = [Total]');");
            // Đơn đã Hoàn thành (Status = 3) coi như đã thu tiền.
            migrationBuilder.Sql("EXEC(N'UPDATE [Orders] SET [IsPaid] = 1, [PaidAt] = [CreatedAt] WHERE [Status] = 3');");
            // Dòng hàng cũ: chép tên sản phẩm hiện tại vào cho hoá đơn có chữ để hiện.
            migrationBuilder.Sql(@"EXEC(N'UPDATE d SET d.[ProductName] = p.[Name]
                                   FROM [OrderDetails] d JOIN [Products] p ON p.[Id] = d.[ProductId]');");

            // ===== Giỏ hàng =====

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ===== Chỉ mục và khoá ngoại =====

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CouponId",
                table: "Orders",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductVariantId",
                table: "OrderDetails",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductVariantId",
                table: "CartItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId_ProductVariantId",
                table: "CartItems",
                columns: new[] { "UserId", "ProductVariantId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_ProductVariants_ProductVariantId",
                table: "OrderDetails",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_ProductVariants_ProductVariantId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CouponId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ProductVariantId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(name: "Subtotal", table: "Orders");
            migrationBuilder.DropColumn(name: "DiscountAmount", table: "Orders");
            migrationBuilder.DropColumn(name: "CouponId", table: "Orders");
            migrationBuilder.DropColumn(name: "CouponCode", table: "Orders");
            migrationBuilder.DropColumn(name: "IsPaid", table: "Orders");
            migrationBuilder.DropColumn(name: "PaidAt", table: "Orders");
            migrationBuilder.DropColumn(name: "CancelReason", table: "Orders");
            migrationBuilder.DropColumn(name: "CancelledAt", table: "Orders");

            migrationBuilder.DropColumn(name: "ProductVariantId", table: "OrderDetails");
            migrationBuilder.DropColumn(name: "ProductName", table: "OrderDetails");
            migrationBuilder.DropColumn(name: "VariantName", table: "OrderDetails");
        }
    }
}
