using osu.Framework.Bindables;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A menu item whose label can be updated after creation.
    /// </summary>
    public interface IDynamicMenuItem
    {
        Bindable<LocalisableString> Label { get; }
    }
}
