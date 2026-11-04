using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FCG.Catalog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Visivel = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JogosAdquiridos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    JogoId = table.Column<int>(type: "int", nullable: false),
                    ValorPago = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataAquisicao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TransacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JogosAdquiridos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JogosAdquiridos_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Ativo", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, true, "Jogos de ação e combate", "Ação" },
                    { 2, true, "Jogos de exploração e aventura", "Aventura" },
                    { 3, true, "Role-Playing Games", "RPG" },
                    { 4, true, "Jogos de estratégia e táticas", "Estratégia" },
                    { 5, true, "Simuladores esportivos", "Esportes" }
                });

            migrationBuilder.InsertData(
                table: "Jogos",
                columns: new[] { "Id", "Ativo", "CategoriaId", "DataCadastro", "Descricao", "Nome", "Preco", "Visivel" },
                values: new object[,]
                {
                    { 1, true, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Batalha futurista entre guerreiros cibernéticos", "Cyber Warriors", 59.90m, true },
                    { 2, true, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Explore um reino perdido repleto de mistérios", "Lost Kingdom", 49.90m, true },
                    { 3, true, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RPG massivo com dragões e magias poderosas", "Dragon Quest Online", 79.90m, true },
                    { 4, true, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Conquiste nações e domine o mundo", "World Conqueror", 39.90m, true },
                    { 5, true, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "O mais completo simulador de futebol", "Soccer Pro 2026", 44.90m, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_CategoriaId",
                table: "Jogos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_JogosAdquiridos_JogoId",
                table: "JogosAdquiridos",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_JogosAdquiridos_UsuarioId_JogoId",
                table: "JogosAdquiridos",
                columns: new[] { "UsuarioId", "JogoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JogosAdquiridos");

            migrationBuilder.DropTable(
                name: "Jogos");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
