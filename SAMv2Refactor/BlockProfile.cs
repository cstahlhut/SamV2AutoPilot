namespace IngameScript
{
    partial class Program
    {
        private class BlockProfile
        { // BlockProfile
            public string[] tags;
            public string[] exclusiveTags;
            public string[] pbAttributes;
            public BlockProfile(ref string[] tags, ref string[] exclusiveTags, ref string[] pbAttributes)
            {
                this.tags = tags;
                this.exclusiveTags = exclusiveTags;
                this.pbAttributes = pbAttributes;
            }

            // Not used
            public string Capitalize(string str)
            {
                foreach (string attribute in pbAttributes)
                {
                    if (attribute.ToLower() == str.ToLower())
                    {
                        return attribute;
                    }
                }
                return "";
            }
        }
    }
}
