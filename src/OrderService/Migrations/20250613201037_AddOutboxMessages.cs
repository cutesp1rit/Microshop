using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Outbox",
                table: "Outbox");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "Outbox");

            migrationBuilder.RenameTable(
                name: "Outbox",
                newName: "OutboxMessages");

            migrationBuilder.RenameColumn(
                name: "Queue",
                table: "OutboxMessages",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Processed",
                table: "OutboxMessages",
                newName: "IsProcessed");

            migrationBuilder.RenameColumn(
                name: "OccurredOn",
                table: "OutboxMessages",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "OutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxMessages",
                table: "OutboxMessages",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OutboxMessages",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "OutboxMessages");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                newName: "Outbox");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Outbox",
                newName: "Queue");

            migrationBuilder.RenameColumn(
                name: "IsProcessed",
                table: "Outbox",
                newName: "Processed");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Outbox",
                newName: "OccurredOn");

            migrationBuilder.AddColumn<byte[]>(
                name: "Body",
                table: "Outbox",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Outbox",
                table: "Outbox",
                column: "Id");
        }
    }
}
