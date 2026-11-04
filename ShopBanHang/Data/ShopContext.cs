using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopBanHang.Models.Entities;

namespace ShopBanHang.Data
{
    /// <summary>
    /// Kế thừa IdentityDbContext để có sẵn các bảng tài khoản của Identity
    /// (AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims,
    /// AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens).
    /// Các bảng nghiệp vụ của shop khai thêm bên dưới.
    /// </summary>
    public class ShopContext : IdentityDbContext<User, Role, int>
    {
        public ShopContext(DbContextOptions<ShopContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<News> News => Set<News>();
        public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Banner> Banners => Set<Banner>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // Phải gọi trước, nếu không các bảng của Identity sẽ không được cấu hình
            base.OnModelCreating(b);

            // Tên bảng số ít cho khớp tên entity
            b.Entity<Category>().ToTable("Categories");
            b.Entity<Brand>().ToTable("Brands");
            b.Entity<Product>().ToTable("Products");
            b.Entity<Order>().ToTable("Orders");
            b.Entity<OrderDetail>().ToTable("OrderDetails");
            b.Entity<News>().ToTable("News");
            b.Entity<ChatConversation>().ToTable("ChatConversations");
            b.Entity<ChatMessage>().ToTable("ChatMessages");
            b.Entity<Banner>().ToTable("Banners");
            b.Entity<Address>().ToTable("Addresses");
            b.Entity<Review>().ToTable("Reviews");
            b.Entity<Coupon>().ToTable("Coupons");
            b.Entity<ProductVariant>().ToTable("ProductVariants");
            b.Entity<CartItem>().ToTable("CartItems");

            // Không cho trùng đường dẫn. Email đã được Identity lo bằng
            // chỉ mục trên NormalizedEmail nên không khai lại ở đây.
            b.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<Brand>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<News>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<Order>().HasIndex(x => x.OrderCode).IsUnique();
            b.Entity<Coupon>().HasIndex(x => x.Code).IsUnique();
            b.Entity<ProductVariant>().HasIndex(v => new { v.ProductId, v.Storage, v.Color }).IsUnique();

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

            // Biến thể đã nằm trong đơn thì không cho xoá (chỉ tắt), để còn hoàn tồn kho khi huỷ đơn
            b.Entity<OrderDetail>()
                .HasOne(d => d.ProductVariant)
                .WithMany()
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mã giảm giá đã dùng thì CouponService không cho xoá; SetNull là để phòng hờ
            b.Entity<Order>()
                .HasOne(o => o.Coupon)
                .WithMany()
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            // Giỏ hàng: xoá tài khoản hoặc xoá biến thể thì dòng giỏ đi theo
            b.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<CartItem>()
                .HasOne(c => c.ProductVariant)
                .WithMany()
                .HasForeignKey(c => c.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mỗi biến thể chỉ có một dòng trong giỏ của một người; thêm nữa thì cộng số lượng
            b.Entity<CartItem>().HasIndex(c => new { c.UserId, c.ProductVariantId }).IsUnique();

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

            // Xoá tài khoản thì xoá luôn sổ địa chỉ của người đó
            b.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mỗi khách chỉ đánh giá một sản phẩm một lần
            b.Entity<Review>().HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();

            // Tra sổ địa chỉ theo chủ sở hữu
            b.Entity<Address>().HasIndex(a => a.UserId);

            // Tra hội thoại của khách lẻ theo cookie, và xếp danh sách theo tin mới nhất
            b.Entity<ChatConversation>().HasIndex(c => c.GuestToken);
            b.Entity<ChatConversation>().HasIndex(c => c.LastMessageAt);
            b.Entity<ChatMessage>().HasIndex(m => new { m.ConversationId, m.CreatedAt });
        }
    }
}
