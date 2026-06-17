using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinAuthorInfo : Container
    {
        private const float height = 50;

        private UpdateableAvatar avatar = null!;
        private FillFlowContainer fields = null!;

        private APIOnlineSkin? skin;
        private int lookupGeneration;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public APIOnlineSkin? Skin
        {
            get => skin;
            set
            {
                if (value == skin)
                    return;

                skin = value;
                Scheduler.AddOnce(updateDisplay);
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;

            Children = new Drawable[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 4,
                    Masking = true,
                    Child = avatar = new UpdateableAvatar(showUserPanelOnHover: true, showGuestOnNull: false)
                    {
                        Size = new Vector2(height),
                    },
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.25f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 4,
                        Offset = new Vector2(0f, 1f),
                    },
                },
                fields = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = height + 5 },
                },
            };

            Scheduler.AddOnce(updateDisplay);
        }

        private void updateDisplay()
        {
            int generation = ++lookupGeneration;

            avatar.User = null;
            fields.Clear();

            if (skin == null)
                return;

            string? uploader = skin.GetUploaderDisplayName();

            if (!string.IsNullOrWhiteSpace(uploader))
                loadUploader(uploader, generation);
            else
                populateFields(null, null);
        }

        private void populateFields(APIUser? uploaderUser, string? uploaderName)
        {
            fields.Clear();

            if (skin == null)
                return;

            if (uploaderUser != null)
            {
                avatar.User = uploaderUser;
                fields.Add(new UserField("uploaded by", uploaderUser, OsuFont.GetFont(weight: FontWeight.Regular, italics: true)));
            }
            else if (!string.IsNullOrWhiteSpace(uploaderName))
            {
                fields.Add(new TextField("uploaded by", uploaderName, OsuFont.GetFont(weight: FontWeight.Regular, italics: true)));
            }

            if (skin.CreatedAt != null)
            {
                fields.Add(new DateField("submitted", skin.CreatedAt.Value, OsuFont.GetFont(weight: FontWeight.Bold))
                {
                    Margin = new MarginPadding { Top = 5 },
                });
            }

            DateTimeOffset? lastUpdated = skin.LastUpdated ?? skin.CreatedAt;

            if (lastUpdated != null)
            {
                fields.Add(new DateField("last updated", lastUpdated.Value, OsuFont.GetFont(weight: FontWeight.Bold)));
            }
        }

        private void loadUploader(string username, int generation)
        {
            populateFields(null, username);

            Task.Run(async () =>
            {
                APIUser? user = null;

                try
                {
                    var request = new GetUserRequest(username);
                    await api.PerformAsync(request).ConfigureAwait(false);
                    user = request.Response;
                }
                catch
                {
                    // Fall back to plain text if the lookup fails.
                }

                Schedule(() =>
                {
                    if (generation != lookupGeneration)
                        return;

                    populateFields(user, username);
                });
            });
        }

        private partial class TextField : FillFlowContainer
        {
            public TextField(string label, string value, FontUsage valueFont)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;

                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = $"{label} ",
                        Font = OsuFont.GetFont(size: 11),
                    },
                    new OsuSpriteText
                    {
                        Text = value,
                        Font = valueFont.With(size: 11),
                    },
                };
            }
        }

        private partial class UserField : FillFlowContainer
        {
            public UserField(string label, APIUser user, FontUsage valueFont)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;

                Child = new LinkFlowContainer(s =>
                {
                    s.Font = OsuFont.GetFont(size: 11);
                }).With(d =>
                {
                    d.AutoSizeAxes = Axes.Both;
                    d.AddText($"{label} ");
                    d.AddUserLink(user, s => s.Font = valueFont.With(size: 11));
                });
            }
        }

        private partial class DateField : FillFlowContainer
        {
            public DateField(string label, DateTimeOffset value, FontUsage valueFont)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;

                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = $"{label} ",
                        Font = OsuFont.GetFont(size: 13),
                    },
                    new DrawableDate(value)
                    {
                        Font = valueFont.With(size: 13),
                    },
                };
            }
        }
    }
}
