namespace GenPhoto.ViewModels
{
    public class ImagePersonViewModel : ViewModelBase
    {
        private bool deleted;

        public bool Deleted
        {
            get => deleted;
            set => SetProperty(ref deleted, value);
        }

        public Guid Id { get; init; }

        public string Name { get; init; }

        public bool IsMatch(string w)
        {
            return Name.Contains(w, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}