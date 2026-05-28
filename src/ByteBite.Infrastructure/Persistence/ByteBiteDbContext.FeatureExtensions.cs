using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Infrastructure.Persistence;

public partial class ByteBiteDbContext
{
    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<ConversationMessage> ConversationMessages { get; set; }

    public virtual DbSet<CustomerStoreVisit> CustomerStoreVisits { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.Username, "uq_customers_username")
                .IsUnique()
                .HasFilter("username IS NOT NULL");

            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasComment("顾客账号名")
                .HasColumnName("username");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Ignore(e => e.PickupCode);

            entity.HasIndex(e => new { e.StoreId, e.PickupCodeValue }, "ix_orders_store_pickup_code_value");
            entity.HasIndex(e => new { e.StoreId, e.PickupCodeValue }, "uq_orders_active_pickup_code_value")
                .IsUnique()
                .HasFilter("status NOT IN ('completed','cancelled','rejected')");

            entity.Property(e => e.PickupCodeValue)
                .HasDefaultValue(0)
                .HasComment("取货码整数码值，页面以6位Base36展示")
                .HasColumnName("pickup_code_value");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasOne(e => e.IndustryCategory)
                .WithMany(e => e.Stores)
                .HasForeignKey(e => e.IndustryCategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("stores_industry_category_id_fkey");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("conversations_pkey");
            entity.ToTable("conversations", tb => tb.HasComment("订单会话表"));

            entity.HasIndex(e => e.OrderId, "uq_conversations_order_id").IsUnique();
            entity.HasIndex(e => new { e.StoreId, e.LastMessageAt }, "ix_conversations_store_last_message");
            entity.HasIndex(e => new { e.CustomerId, e.LastMessageAt }, "ix_conversations_customer_last_message");
            entity.HasIndex(e => e.DeviceId, "ix_conversations_device_id");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DeviceId).HasMaxLength(200).HasColumnName("device_id");
            entity.Property(e => e.LastMessageAt).HasDefaultValueSql("now()").HasColumnName("last_message_at");
            entity.Property(e => e.CustomerUnreadCount).HasDefaultValue(0).HasColumnName("customer_unread_count");
            entity.Property(e => e.MerchantUnreadCount).HasDefaultValue(0).HasColumnName("merchant_unread_count");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");

            entity.HasOne(e => e.Order).WithOne().HasForeignKey<Conversation>(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Store).WithMany().HasForeignKey(e => e.StoreId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ConversationMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("conversation_messages_pkey");
            entity.ToTable("conversation_messages", tb => tb.HasComment("订单会话消息表"));

            entity.HasIndex(e => new { e.ConversationId, e.CreatedAt }, "ix_conversation_messages_conversation_created");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.SenderType).HasMaxLength(20).HasColumnName("sender_type");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Content).HasMaxLength(1000).HasColumnName("content");
            entity.Property(e => e.MessageType).HasMaxLength(20).HasDefaultValueSql("'text'::character varying").HasColumnName("message_type");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

            entity.HasOne(e => e.Conversation).WithMany(e => e.Messages).HasForeignKey(e => e.ConversationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerStoreVisit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customer_store_visits_pkey");
            entity.ToTable("customer_store_visits", tb => tb.HasComment("顾客最近访问店铺表"));

            entity.HasIndex(e => new { e.CustomerId, e.StoreId }, "uq_customer_store_visits_customer_store")
                .IsUnique()
                .HasFilter("customer_id IS NOT NULL");
            entity.HasIndex(e => new { e.DeviceId, e.StoreId }, "uq_customer_store_visits_device_store")
                .IsUnique()
                .HasFilter("device_id IS NOT NULL");
            entity.HasIndex(e => e.LastVisitedAt, "ix_customer_store_visits_last_visited");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DeviceId).HasMaxLength(200).HasColumnName("device_id");
            entity.Property(e => e.LastVisitedAt).HasDefaultValueSql("now()").HasColumnName("last_visited_at");
            entity.Property(e => e.LastOrderedAt).HasColumnName("last_ordered_at");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");

            entity.HasOne(e => e.Store).WithMany().HasForeignKey(e => e.StoreId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
