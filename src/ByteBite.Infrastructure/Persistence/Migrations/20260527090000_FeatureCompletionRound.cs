using System;
using ByteBite.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteBite.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ByteBiteDbContext))]
    [Migration("20260527090000_FeatureCompletionRound")]
    public partial class FeatureCompletionRound : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS ix_orders_pickup_code;
                ALTER TABLE orders DROP CONSTRAINT IF EXISTS uq_orders_pickup_code_store;
                DROP INDEX IF EXISTS uq_orders_pickup_code_store;
                """);

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "顾客账号名");

            migrationBuilder.AddColumn<int>(
                name: "pickup_code_value",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "取货码整数码值，页面以6位Base36展示");

            migrationBuilder.AlterColumn<string>(
                name: "pickup_code",
                table: "orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                comment: "历史取货码兼容字段",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldComment: "取货码（4-6位字母数字）");

            migrationBuilder.Sql("""
                WITH numbered AS (
                    SELECT id, ROW_NUMBER() OVER (PARTITION BY store_id ORDER BY created_at, id)::integer AS rn
                    FROM orders
                    WHERE pickup_code_value = 0
                )
                UPDATE orders o
                SET pickup_code_value = numbered.rn
                FROM numbered
                WHERE o.id = numbered.id;
                """);

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_message_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    customer_unread_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    merchant_unread_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("conversations_pkey", x => x.id);
                    table.ForeignKey("fk_conversations_customers_customer_id", x => x.customer_id, "customers", "id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("fk_conversations_orders_order_id", x => x.order_id, "orders", "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("fk_conversations_stores_store_id", x => x.store_id, "stores", "id", onDelete: ReferentialAction.Cascade);
                },
                comment: "订单会话表");

            migrationBuilder.CreateTable(
                name: "customer_store_visits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_visited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_ordered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("customer_store_visits_pkey", x => x.id);
                    table.ForeignKey("fk_customer_store_visits_customers_customer_id", x => x.customer_id, "customers", "id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("fk_customer_store_visits_stores_store_id", x => x.store_id, "stores", "id", onDelete: ReferentialAction.Cascade);
                },
                comment: "顾客最近访问店铺表");

            migrationBuilder.CreateTable(
                name: "conversation_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    message_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'text'::character varying"),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("conversation_messages_pkey", x => x.id);
                    table.ForeignKey("fk_conversation_messages_conversations_conversation_id", x => x.conversation_id, "conversations", "id", onDelete: ReferentialAction.Cascade);
                },
                comment: "订单会话消息表");

            migrationBuilder.CreateIndex("uq_customers_username", "customers", "username", unique: true, filter: "username IS NOT NULL");
            migrationBuilder.CreateIndex("ix_orders_store_pickup_code_value", "orders", new[] { "store_id", "pickup_code_value" });
            migrationBuilder.CreateIndex("uq_orders_active_pickup_code_value", "orders", new[] { "store_id", "pickup_code_value" }, unique: true, filter: "status NOT IN ('completed','cancelled','rejected')");
            migrationBuilder.CreateIndex("ix_conversations_customer_last_message", "conversations", new[] { "customer_id", "last_message_at" });
            migrationBuilder.CreateIndex("ix_conversations_device_id", "conversations", "device_id");
            migrationBuilder.CreateIndex("ix_conversations_store_last_message", "conversations", new[] { "store_id", "last_message_at" });
            migrationBuilder.CreateIndex("uq_conversations_order_id", "conversations", "order_id", unique: true);
            migrationBuilder.CreateIndex("ix_conversation_messages_conversation_created", "conversation_messages", new[] { "conversation_id", "created_at" });
            migrationBuilder.CreateIndex("ix_customer_store_visits_last_visited", "customer_store_visits", "last_visited_at");
            migrationBuilder.CreateIndex("uq_customer_store_visits_customer_store", "customer_store_visits", new[] { "customer_id", "store_id" }, unique: true, filter: "customer_id IS NOT NULL");
            migrationBuilder.CreateIndex("uq_customer_store_visits_device_store", "customer_store_visits", new[] { "device_id", "store_id" }, unique: true, filter: "device_id IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "conversation_messages");
            migrationBuilder.DropTable(name: "customer_store_visits");
            migrationBuilder.DropTable(name: "conversations");
            migrationBuilder.DropIndex(name: "uq_customers_username", table: "customers");
            migrationBuilder.DropIndex(name: "ix_orders_store_pickup_code_value", table: "orders");
            migrationBuilder.DropIndex(name: "uq_orders_active_pickup_code_value", table: "orders");
            migrationBuilder.DropColumn(name: "username", table: "customers");
            migrationBuilder.DropColumn(name: "pickup_code_value", table: "orders");
            migrationBuilder.AlterColumn<string>(
                name: "pickup_code",
                table: "orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                comment: "取货码（4-6位字母数字）",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true,
                oldComment: "历史取货码兼容字段");

            migrationBuilder.CreateIndex(
                name: "ix_orders_pickup_code",
                table: "orders",
                column: "pickup_code");

            migrationBuilder.CreateIndex(
                name: "uq_orders_pickup_code_store",
                table: "orders",
                columns: new[] { "store_id", "pickup_code" },
                unique: true);
        }
    }
}
