using Smlp.Base.Reward.Enums;

namespace Smlp.Base.Reward.Lists
{
    public static class RewardList
    {
        public static IList<RewardType> RewardTypeLists()
        {
            var list = new List<RewardType>
            {
                RewardType.DiscountPercent,
                RewardType.DiscountAmount,
                RewardType.FreeItem,
                RewardType.Voucher
            };

            return list;
        }
    }
}
