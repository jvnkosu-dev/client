// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using System;
using System.Net.Http;

namespace osu.Game.Online.API.Requests
{
    public class CommentVoteRequest : APIRequest<CommentBundle>
    {
        private readonly long id;
        private readonly CommentVoteAction action;
        private readonly string? commentableType;

        public CommentVoteRequest(long id, CommentVoteAction action, string? commentableType = null)
        {
            this.id = id;
            this.action = action;
            this.commentableType = commentableType;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = action == CommentVoteAction.Vote ? HttpMethod.Post : HttpMethod.Delete;
            return req;
        }

        protected override string Uri => isSkinComment
            ? $@"{API!.Endpoints.APIUrl}/api/skins/comments/{id}/vote"
            : base.Uri;

        protected override string Target => $@"comments/{id}/vote";

        private bool isSkinComment =>
            string.Equals(commentableType, @"skin", StringComparison.OrdinalIgnoreCase);
    }

    public enum CommentVoteAction
    {
        Vote,
        UnVote
    }
}
