using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopBanHang.Migrations
{
    /// <inheritdoc />
    public partial class ThemThongSoSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Products",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GalleryUrls",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Os",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ram",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Storage",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GalleryUrls",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Os",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Ram",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Storage",
                table: "Products");
        }
    }
}
