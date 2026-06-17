namespace osu.Game.Skinning
{
    public readonly struct SkinUploadResult
    {
        public bool Success { get; init; }

        public string? ErrorMessage { get; init; }

        public int? AssignedOnlineSkinId { get; init; }

        public static SkinUploadResult Completed(int? onlineSkinId = null) => new SkinUploadResult
        {
            Success = true,
            AssignedOnlineSkinId = onlineSkinId,
        };

        public static SkinUploadResult Failed(string message) => new SkinUploadResult
        {
            Success = false,
            ErrorMessage = message,
        };
    }
}
