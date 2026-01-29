namespace STool.STag
{
    public static class STagUtility
    {
        public static bool HasTag(this ISTagHolder holder, STagData data)
        {
            return holder.TagData.Contains(data);
        }
        
    }
}
