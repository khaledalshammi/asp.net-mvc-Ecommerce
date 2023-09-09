
namespace Ecommerce.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }
        IRoleRepository Role { get; }
        IProductRepository Product { get; }
        ICategoryRepository Category { get; }
        IReviewRepository Review { get; }
        ILikeRepository Like { get; }
        IDisLikeRepository DisLike { get; }
        ICommentRepository Comment { get; }
        ICommentImagesRepository CommentImages { get; }
        INewsRepository News { get; }
        INotificationRepository Notification { get; }
        IImageRepository Image { get; }
        IApplicantRepository Applicant { get; }
        IJobRepository Job { get; }
        ICartRepository Cart { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IInquiryRepository Inquiry { get; }
        IOrderRepository Order { get; }
        IDiscountRepository Discount { get; }
        ISubscriptionRepository Subscription { get; }
        ISubscriptionTypeRepository SubscriptionType { get; }
        void Save();
    }
}
