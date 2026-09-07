// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.API.Requests
{
    public abstract class MarathonServiceRequest : APIRequest
    {
        protected abstract string Path { get; }

        protected sealed override string Uri => $@"{API!.Endpoints.MarathonUrl}{Path}";

        protected override string Target => Path.TrimStart('/');
    }

    public abstract class MarathonServiceRequest<T> : APIRequest<T>
        where T : class
    {
        protected abstract string Path { get; }

        protected sealed override string Uri => $@"{API!.Endpoints.MarathonUrl}{Path}";

        protected override string Target => Path.TrimStart('/');
    }
}
