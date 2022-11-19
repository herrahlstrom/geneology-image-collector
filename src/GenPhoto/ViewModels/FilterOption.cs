namespace GenPhoto.ViewModels
{
    class FilterOption : ViewModelBase
    {
        private string? _selectedOption;

        public FilterOption()
        {
            PropertyChanged += (_, _) => { SelectedOptionChangedCallback?.Invoke(); };
        }
        public required string Key { get; init; }
        public required string Name { get; init; }
        public required IList<KeyValuePair<string, string>> Options { get; init; }
        public string? SelectedOption { get => _selectedOption; set => SetProperty(ref _selectedOption, value); }
        public required Action SelectedOptionChangedCallback { get; set; }
    }
}