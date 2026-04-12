namespace SMLP.Base.Common.Attributes
{
    public class CssAttribute : Attribute
    {
        public string Name { get; private set; }

        public CssAttribute(string name)
        {
            Name = name;
        }
    }
}
