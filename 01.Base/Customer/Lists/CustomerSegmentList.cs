using Smlp.Base.Customer.Enums;

namespace Smlp.Base.Customer.Lists
{
    public static class CustomerSegmentList
    {
        public static IList<CustomerSegment> CustomerSegmentLists()
        {
            var list = new List<CustomerSegment>
            {
                CustomerSegment.New,
                CustomerSegment.Active,
                CustomerSegment.Loyal,
                CustomerSegment.Inactive,
                CustomerSegment.AtRisk,
                CustomerSegment.Lost
            };

            return list;
        }
    }
}
