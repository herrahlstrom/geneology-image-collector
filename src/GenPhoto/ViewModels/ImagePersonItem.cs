using GenPhoto.Infrastructure;

namespace GenPhoto.ViewModels
{
    public class ImagePersonViewModel : ViewModelBase
    {
        private EntityState _state;

        public EntityState State { get => _state; set => SetProperty(ref _state, value); }

        public Guid Id { get; init; }

        public required string Name { get; init; }

        public bool IsMatch(string w)
        {
            return Name.Contains(w, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}