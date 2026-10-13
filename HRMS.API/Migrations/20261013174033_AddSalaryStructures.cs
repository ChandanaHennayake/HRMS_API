using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HRMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryStructures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalaryComponent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ComponentType = table.Column<short>(type: "smallint", nullable: false),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false),
                    IsEpfApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    IsFixed = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryComponent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalaryStructure",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryStructure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSalary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    SalaryStructureId = table.Column<long>(type: "bigint", nullable: true),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    PaymentMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankAccountNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSalary_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeSalary_SalaryStructure_SalaryStructureId",
                        column: x => x.SalaryStructureId,
                        principalTable: "SalaryStructure",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalaryStructureComponent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalaryStructureId = table.Column<long>(type: "bigint", nullable: false),
                    SalaryComponentId = table.Column<long>(type: "bigint", nullable: false),
                    DefaultAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PercentageOfBasic = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryStructureComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryStructureComponent_SalaryComponent_SalaryComponentId",
                        column: x => x.SalaryComponentId,
                        principalTable: "SalaryComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalaryStructureComponent_SalaryStructure_SalaryStructureId",
                        column: x => x.SalaryStructureId,
                        principalTable: "SalaryStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSalaryComponent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeSalaryId = table.Column<long>(type: "bigint", nullable: false),
                    SalaryComponentId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalaryComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSalaryComponent_EmployeeSalary_EmployeeSalaryId",
                        column: x => x.EmployeeSalaryId,
                        principalTable: "EmployeeSalary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeSalaryComponent_SalaryComponent_SalaryComponentId",
                        column: x => x.SalaryComponentId,
                        principalTable: "SalaryComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalary_EmployeeId",
                table: "EmployeeSalary",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalary_SalaryStructureId",
                table: "EmployeeSalary",
                column: "SalaryStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryComponent_SalaryComponentId",
                table: "EmployeeSalaryComponent",
                column: "SalaryComponentId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmployeeSalary_Component",
                table: "EmployeeSalaryComponent",
                columns: new[] { "EmployeeSalaryId", "SalaryComponentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_SalaryComponent_Company_Code",
                table: "SalaryComponent",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructureComponent_SalaryComponentId",
                table: "SalaryStructureComponent",
                column: "SalaryComponentId");

            migrationBuilder.CreateIndex(
                name: "UQ_Structure_Component",
                table: "SalaryStructureComponent",
                columns: new[] { "SalaryStructureId", "SalaryComponentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSalaryComponent");

            migrationBuilder.DropTable(
                name: "SalaryStructureComponent");

            migrationBuilder.DropTable(
                name: "EmployeeSalary");

            migrationBuilder.DropTable(
                name: "SalaryComponent");

            migrationBuilder.DropTable(
                name: "SalaryStructure");
        }
    }
}
