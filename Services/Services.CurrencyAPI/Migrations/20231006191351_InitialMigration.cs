using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Services.CurrencyAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    symbol = table.Column<string>(type: "TEXT", nullable: false),
                    current_price = table.Column<decimal>(type: "TEXT", nullable: true),
                    market_cap = table.Column<decimal>(type: "TEXT", nullable: true),
                    market_cap_rank = table.Column<int>(type: "INTEGER", nullable: true),
                    price_change_24h = table.Column<decimal>(type: "TEXT", nullable: true),
                    price_change_percentage_24h = table.Column<double>(type: "REAL", nullable: true),
                    image = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "Currency",
                columns: new[] { "id", "current_price", "image", "market_cap", "market_cap_rank", "name", "price_change_24h", "price_change_percentage_24h", "symbol" },
                values: new object[,]
                {
                    { "bitcoin", null, null, null, null, "Bitcoin", null, null, "btc" },
                    { "ethereum", null, null, null, null, "Ethereum", null, null, "eth" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Currency");
        }
    }
}
