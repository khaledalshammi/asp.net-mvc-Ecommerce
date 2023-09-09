using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Models.ViewModels
{
    public class SubscriptionTypeVM
    {
        public List<SubscriptionType> SubscriptionTypes { get; set; }
        public User User { get; set; }
    }
}
