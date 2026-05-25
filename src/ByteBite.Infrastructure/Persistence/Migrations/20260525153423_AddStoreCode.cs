using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteBite.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "store_code",
                table: "stores",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "",
                comment: "店铺码（6位Base36编码，唯一标识，用于短链分享）");

            migrationBuilder.CreateIndex(
                name: "uq_stores_store_code",
                table: "stores",
                column: "store_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_stores_store_code",
                table: "stores");

            migrationBuilder.DropColumn(
                name: "store_code",
                table: "stores");
        }
    }
}
