using Smlp.Base.Customer.Enums;

namespace Smlp.Base.Customer.Lists
{
    public static class CustomerTierList
    {
        public static IList<CustomerTier> CustomerTierLists()
        {
            var list = new List<CustomerTier>
            {
                CustomerTier.Bronze,
                CustomerTier.Silver,
                CustomerTier.Gold,
                CustomerTier.Platinum
            };

            return list;
        }
    }
}
