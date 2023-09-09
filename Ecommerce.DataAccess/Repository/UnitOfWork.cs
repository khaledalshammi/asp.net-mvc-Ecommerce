using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;

namespace Ecommerce.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public IUserRepository User { get; set; }
        public IRoleRepository Role { get; set; }
        public IProductRepository Product { get; set; }
        public ICategoryRepository Category { get; set; }
        public IReviewRepository Review { get; set; }
        public IDisLikeRepository DisLike { get; set; }
        public ILikeRepository Like { get; set; }
        public ICommentRepository Comment { get; set; }
        public ICommentImagesRepository CommentImages { get; set; }
        public INewsRepository News { get; set; }
        public INotificationRepository Notification { get; set; }
        public IImageRepository Image { get; set; }
        public IApplicantRepository Applicant { get; set; }
        public IJobRepository Job { get; set; }
        public ICartRepository Cart { get; set; }
        public IShoppingCartRepository ShoppingCart { get; set; }
        public IInquiryRepository Inquiry { get; set; }
        public IOrderRepository Order { get; set; }
        public IDiscountRepository Discount { get; set; }
        public ISubscriptionRepository Subscription { get; set; }
        public ISubscriptionTypeRepository SubscriptionType { get; set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            User = new UserRepository(_db);
            Role = new RoleRepository(_db);
            Product = new ProductRepository(_db);
            Category = new CategoryRepository(_db);
            Review = new ReviewRepository(_db);
            Like = new LikeRepository(_db);
            DisLike = new DisLikeRepository(_db);
            Comment = new CommentRepository(_db);
            CommentImages = new CommentImagesRepository(_db);
            News = new NewsRepository(_db);
            Notification = new NotificationRepository(_db);
            Image = new ImageRepository(_db);
            Applicant = new ApplicantRepository(_db);
            Job = new JobRepository(_db);
            Cart = new CartRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            Inquiry = new InquiryRepository(_db);
            Order = new OrderRepository(_db);
            Discount = new DiscountRepository(_db);
            Subscription = new SubscriptionRepository(_db);
            SubscriptionType = new SubscriptionTypeRepository(_db);
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
