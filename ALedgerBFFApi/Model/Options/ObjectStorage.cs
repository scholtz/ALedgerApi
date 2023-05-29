namespace ALedgerBFFApi.Model.Options
{
    public class ObjectStorage
    {
        public string Type { get; set; } = "AWS";
        public string Host { get; set; }
        public string Bucket { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
    }
}
