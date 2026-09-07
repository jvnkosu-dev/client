// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.Comments;

namespace osu.Game.Overlays.SkinSet
{
    /// <summary>
    /// Comments on a skin page. Talks to osu-skins-service
    /// (<c>/api/skins/{id}/comments</c>), not osu-web.
    /// </summary>
    public partial class SkinCommentsSection : Container
    {
        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        private readonly CommentsContainer comments;

        public SkinCommentsSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Child = new BeatmapSetLayoutSection
            {
                Child = comments = new CommentsContainer(),
            };

            Skin.BindValueChanged(skin =>
            {
                if (skin.NewValue?.OnlineID > 0)
                {
                    comments.ShowComments(CommentableType.Skin, skin.NewValue.OnlineID);
                    Show();
                }
                else
                {
                    Hide();
                }
            }, true);
        }
    }
}
