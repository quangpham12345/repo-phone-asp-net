using Microsoft.EntityFrameworkCore;
using ShopBanHang.Models.Entities;

namespace ShopBanHang.Data
{
    public class ShopContext : DbContext
    {
        public ShopContext(DbContextOptions<ShopContext> options) : base(options) { }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<News> News => Set<News>();
        public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Banner> Banners => Set<Banner>();
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // Tên bảng số ít cho khớp tên entity
            b.Entity<Role>().ToTable("Roles");
            b.Entity<User>().ToTable("Users");
            b.Entity<Category>().ToTable("Categories");
            b.Entity<Brand>().ToTable("Brands");
            b.Entity<Product>().ToTable("Products");
            b.Entity<Order>().ToTable("Orders");
            b.Entity<OrderDetail>().ToTable("OrderDetails");
            b.Entity<News>().ToTable("News");
            b.Entity<ChatConversation>().ToTable("ChatConversations");
            b.Entity<ChatMessage>().ToTable("ChatMessages");
            b.Entity<Banner>().ToTable("Banners");
            b.Entity<Coupon>().ToTable("Coupons");
            b.Entity<ProductVariant>().ToTable("ProductVariants");

            // Không cho trùng: mỗi email một tài khoản, mỗi slug một đường dẫn
            b.Entity<Role>().HasIndex(x => x.Name).IsUnique();
            b.Entity<User>().HasIndex(x => x.Email).IsUnique();
            b.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<Brand>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<News>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<Order>().HasIndex(x => x.OrderCode).IsUnique();
            b.Entity<Coupon>().HasIndex(x => x.Code).IsUnique();
            b.Entity<ProductVariant>().HasIndex(v => new { v.ProductId, v.Storage, v.Color }).IsUnique();

            // Xoá vai trò đang có người dùng thì chặn, không xoá lan sang Users
            b.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Đơn của khách không đăng nhập thì UserId null
            b.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Xoá đơn thì xoá luôn các dòng hàng của đơn đó
            b.Entity<OrderDetail>()
                .HasOne(d => d.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Sản phẩm đã nằm trong đơn thì không cho xoá, để không vỡ lịch sử mua hàng
            b.Entity<OrderDetail>()
                .HasOne(d => d.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(br => br.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hội thoại của khách lẻ có UserId null, nên tắt tài khoản không mất đoạn chat
            b.Entity<ChatConversation>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Xoá hội thoại thì xoá luôn tin nhắn của nó
            b.Entity<ChatMessage>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tra hội thoại của khách lẻ theo cookie, và xếp danh sách theo tin mới nhất
            b.Entity<ChatConversation>().HasIndex(c => c.GuestToken);
            b.Entity<ChatConversation>().HasIndex(c => c.LastMessageAt);
            b.Entity<ChatMessage>().HasIndex(m => new { m.ConversationId, m.CreatedAt });
        }
    }
}
