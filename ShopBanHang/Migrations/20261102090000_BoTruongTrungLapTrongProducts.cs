using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopBanHang.Migrations
{
    /// <summary>
    /// Bỏ 5 cột trùng với ProductVariants khỏi bảng Products: Price, OldPrice, Stock, Storage, Color.
    /// Từ giờ giá, tồn kho, dung lượng và màu chỉ nằm ở biến thể.
    ///
    /// Không làm mất dữ liệu: sản phẩm nào chưa có biến thể thì được tạo một biến thể từ chính các
    /// cột cũ trước khi cột bị xoá. Sản phẩm đã có biến thể thì Products.Price/OldPrice/Stock vốn
    /// chỉ là số tổng hợp từ biến thể, xoá đi không mất gì.
    /// </summary>
    public partial class BoTruongTrungLapTrongProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dung lượng hoặc màu bị bỏ trống (ví dụ phụ kiện) thì điền nhãn mặc định,
            // vì biến thể bắt buộc có cả hai.
            migrationBuilder.Sql(@"
INSERT INTO ProductVariants (ProductId, Storage, Color, Price, OldPrice, Stock, ImageUrl, IsActive)
SELECT p.Id,
       COALESCE(NULLIF(LTRIM(RTRIM(p.Storage)), N''), N'Tiêu chuẩn'),
       COALESCE(NULLIF(LTRIM(RTRIM(p.Color)), N''), N'Mặc định'),
       p.Price,
       p.OldPrice,
       p.Stock,
       NULL,
       1
FROM Products p
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants v WHERE v.ProductId = p.Id);");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OldPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Storage",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Products",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OldPrice",
                table: "Products",
                type: "decimal(18,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Storage",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            // Tính lại 5 cột cũ từ biến thể đang bán, cùng luật với TongHopBienThe:
            // giá thấp nhất (cùng giá thì lấy giá gốc cao hơn), tổng tồn kho, dung lượng và màu của biến thể rẻ nhất.
            migrationBuilder.Sql(@"
UPDATE p SET
    p.Price = ISNULL(r.Price, 0),
    p.OldPrice = r.OldPrice,
    p.Storage = r.Storage,
    p.Color = r.Color,
    p.Stock = ISNULL((SELECT SUM(v.Stock) FROM ProductVariants v WHERE v.ProductId = p.Id AND v.IsActive = 1), 0)
FROM Products p
OUTER APPLY (
    SELECT TOP 1 v.Price, v.OldPrice, v.Storage, v.Color
    FROM ProductVariants v
    WHERE v.ProductId = p.Id
    ORDER BY v.IsActive DESC, v.Price ASC, ISNULL(v.OldPrice, 0) DESC
) r;");
        }
    }
}
