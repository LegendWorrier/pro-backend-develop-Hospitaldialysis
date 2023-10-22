namespace Wasenshi.HemoDialysisPro.Models
{
    public class FileEntry : EntityBase<string>
    {
        public string Uri { get; set; }
        public string ContentType { get; set; }
    }
}