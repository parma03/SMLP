using Smlp.Base.Customer.Enums;

namespace Smlp.Base.Customer.Lists
{
    public static class GenderList
    {
        public static IList<Gender> GenderLists()
        {
            var list = new List<Gender>
            {
                Gender.LakiLaki,
                Gender.Perempuan,

            };

            return list;
        }
    }
}
