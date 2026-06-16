using System.Net.Http;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public class PostSkinFavouriteRequest : APIRequest
    {
        public readonly SkinFavouriteAction Action;

        private readonly int id;

        public PostSkinFavouriteRequest(int id, SkinFavouriteAction action)
        {
            this.id = id;
            Action = action;
        }

        // action is passed via the query string because osu!'s WebRequest encodes form parameters as multipart/form-data,
        // while the skin service reads standard url-encoded/query values via ParseForm.
        protected override string Uri => $"https://osu.jvnko.boats/api/skins/{id}/favourites?action={getActionString()}";

        protected override string Target => string.Empty;

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            return req;
        }

        private string getActionString() => Action == SkinFavouriteAction.Favourite ? "favourite" : "unfavourite";
    }

    public enum SkinFavouriteAction
    {
        Favourite,
        UnFavourite,
    }
}
