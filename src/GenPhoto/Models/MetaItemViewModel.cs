using GenPhoto.Infrastructure;
using System.Diagnostics;

namespace GenPhoto.Models;

[DebuggerDisplay("{Key} ({DisplayKey}) {Value}")]
public record MetaItemViewModel
{
    private string m_value = "";

    public MetaItemViewModel(string key, string value, EntityState state = EntityState.Unmodified)
    {
        Key = key;
        DisplayKey = key;
        m_value = value;
        State = state;
    }

    public string DisplayKey { get; init; }
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