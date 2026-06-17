using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.SkinListing.Submission;
using osu.Game.Screens.Edit.Components.Menus;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinEditorUploadMenuItem : EditorMenuItem, IDynamicMenuItem
    {
        public Bindable<LocalisableString> Label { get; } = new Bindable<LocalisableString>(SkinMetadataHelper.UploadActionText);

        public SkinEditorUploadMenuItem(Action action)
            : base(SkinMetadataHelper.UploadActionText, MenuItemType.Standard, action)
        {
        }
    }
}
