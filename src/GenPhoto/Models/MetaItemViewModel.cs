using GenPhoto.Infrastructure;
using GenPhoto.Shared;
using System.Diagnostics;

namespace GenPhoto.Models;

[DebuggerDisplay("{Key} ({DisplayKey}) {Value}")]
public record MetaItemViewModel
{
    private string m_value = "";

    public MetaItemViewModel(string key, string value, EntityState state = EntityState.Unmodified)
    {
        Key = key;
        m_value = value;
        State = state;
    }

    public string DisplayKey => Key switch
    {
        nameof(ImageMetaKey.Repository) => "Arkiv",
        nameof(ImageMetaKey.Volume) => "Volym",
        nameof(ImageMetaKey.Year) => "År",
        nameof(ImageMetaKey.Image) => "Bild",
        nameof(ImageMetaKey.Page) => "Sida",
        nameof(ImageMetaKey.Location) => "Plats",
        nameof(ImageMetaKey.Reference) => "Referens",
        _ => Key
    };

    public int Sort => Key switch
    {
        nameof(ImageMetaKey.Repository) => 1,
        nameof(ImageMetaKey.Volume) => 2,
        nameof(ImageMetaKey.Year) => 3,
        nameof(ImageMetaKey.Image) => 4,
        nameof(ImageMetaKey.Page) => 5,
        nameof(ImageMetaKey.Location) => 6,
        nameof(ImageMetaKey.Reference) => 7,
        _ => 99
    };
    public string Key { get; init; }
    public EntityState State { get; set; } = EntityState.Unmodified;

    public static implicit operator KeyValuePair<string, string>(MetaItemViewModel meta)
    {
        return new KeyValuePair<string, string>(meta.Key, meta.Value);
    }

    public string Value
    {
        get => m_value;
        set
        {
            m_value = value;
            if (string.IsNullOrWhiteSpace(m_value))
            {
                State = EntityState.Deleted;
            }
            else
            {
                State = EntityState.Modified;
            }
        }
    }
}