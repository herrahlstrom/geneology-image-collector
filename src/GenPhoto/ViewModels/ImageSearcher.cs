using GenPhoto.Extensions;
using GenPhoto.Infrastructure;
using GenPhoto.Models;
using GenPhoto.Shared;

namespace GenPhoto.ViewModels;

class ImageSearcher
{
    private string m_filterText = "";
 
    private string[] m_searchFilterArray = Array.Empty<string>();

    private static bool FilterByOptions(ImageViewModel item, FilterOption filterOption)
    {
        if (string.IsNullOrEmpty(filterOption.SelectedOption))
        {
            return true;
        }
        else if (filterOption.Key == "person" && Guid.TryParse(filterOption.SelectedOption, out var personId))
        {
            if (personId == Guid.Empty || item.HasPerson(personId))
            {
                return true;
            }
        }
        else if (filterOption.Key.StartsWith("meta."))
        {
            var key = filterOption.Key[5..];
            if (item.HasMetaValue(key, filterOption.SelectedOption))
            {
                return true;
            }
        }

        return false;
    }

    public void BeforeFilter()
    {
        m_searchFilterArray = string.IsNullOrWhiteSpace(m_filterText)
            ? Array.Empty<string>()
            : m_filterText.Split(" ", StringSplitOptions.RemoveEmptyEntries);
    }

    public IEnumerable<FilterOption> BuildFilterOptions(IList<ImageViewModel> filteredItems, Action selectedOptionChangedCallback)
    {
        // Filter option | Person
        yield return new FilterOption()
        {
            Key = "person",
            Name = "Person",
            Options = filteredItems
                    .SelectMany(x => x.Persons.OfType<ImagePersonViewModel>().Select(y => new { y.Id, y.Name }))
                    .Append(new { Id = Guid.Empty, Name = "" })
                    .Distinct()
                    .OrderBy(x => x.Name)
                    .Select(x => new KeyValuePair<string, string>(x.Id.ToString(), x.Name))
                    .ToList(),
            SelectedOptionChangedCallback = selectedOptionChangedCallback
        };

        // Filter option | Repository
        yield return new FilterOption()
        {
            Key = $"meta.{ImageMetaKey.Repository}",
            Name = "Arkiv",
            Options = filteredItems
                .SelectMany(x => x.Meta.OfType<MetaItemViewModel>())
                .Where(x => x.Key == nameof(ImageMetaKey.Repository))
                .Select(x => x.Value)
                .Append("")
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new KeyValuePair<string, string>(x, x))
                .ToList(),
            SelectedOptionChangedCallback = selectedOptionChangedCallback
        };

        // Filter option | Volume
        yield return new FilterOption()
        {
            Key = $"meta.{ImageMetaKey.Volume}",
            Name = "Volym",
            Options = filteredItems
              .SelectMany(x => x.Meta.OfType<MetaItemViewModel>())
              .Where(x => x.Key == nameof(ImageMetaKey.Volume))
              .Select(x => x.Value)
              .Append("")
              .Distinct()
              .OrderBy(x => x)
              .Select(x => new KeyValuePair<string, string>(x, x))
              .ToList(),
            SelectedOptionChangedCallback = selectedOptionChangedCallback
        };
    }

    public bool FilterItem(ImageViewModel item, ICollection<FilterOption> filterOptions)
    {
        if (m_searchFilterArray.Length == 0)
        {
            return false;
        }

        if (m_searchFilterArray.All(item.IsMatch) &&
            filterOptions.All(filterOption => FilterByOptions(item, filterOption)))
        {
            return true;
        }

        return false;

    }
    public string FilterText { get => m_filterText; set => m_filterText = value; }
}
