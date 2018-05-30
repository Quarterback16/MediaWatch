namespace MediaWatch.Models
{
    public class FolderItem
    {
        public string Key { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Key;
        }
    }
}