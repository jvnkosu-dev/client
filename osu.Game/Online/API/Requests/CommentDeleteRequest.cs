// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class CommentDeleteRequest : APIRequest<CommentBundle>
    {
        public readonly long CommentId;

        private readonly string? commentableType;

        public CommentDeleteRequest(long id, string? commentableType = null)
        {
            CommentId = id;
            this.commentableType = commentableType;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Delete;
            return req;
        }

        protected override string Uri => isSkinComment
            ? $@"{API!.Endpoints.APIUrl}/api/skins/comments/{CommentId}"
            : base.Uri;

        protected override string Target => $@"comments/{CommentId}";

        private bool isSkinComment =>
            string.Equals(commentableType, @"skin", StringComparison.OrdinalIgnoreCase);
    }
}
