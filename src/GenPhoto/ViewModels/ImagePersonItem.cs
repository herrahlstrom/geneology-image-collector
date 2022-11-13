namespace GenPhoto.ViewModels
{
    public class ImagePersonItem
    {
        public required Guid Id { get; init; }

        public required string Name { get; init; }

        public bool IsMatch(string w)
        {
            return Name.Contains(w, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}