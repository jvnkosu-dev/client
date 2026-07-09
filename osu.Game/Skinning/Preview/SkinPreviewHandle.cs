// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// Owns a temporarily loaded skin and any resources required to keep it alive.
    /// </summary>
    public sealed class SkinPreviewHandle : IDisposable
    {
        public Skin Skin { get; }

        private readonly IDisposable[] disposables;

        public SkinPreviewHandle(Skin skin, params IDisposable[] disposables)
        {
            Skin = skin;
            this.disposables = disposables;
        }

        public void Dispose()
        {
            Skin.Dispose();

            foreach (var disposable in disposables)
                disposable.Dispose();
        }
    }
}
