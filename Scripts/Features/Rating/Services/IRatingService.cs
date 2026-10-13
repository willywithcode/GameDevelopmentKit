namespace GameFoundation.Scripts.Features.Rating.Services
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// How a review request ended. Deliberately says nothing about what the player answered:
    /// neither Google Play nor the App Store reports whether a review was written, or even whether
    /// their sheet appeared, so no caller may gate a reward or a retry on this.
    /// </summary>
    public enum ReviewFlowResult
    {
        /// <summary>
        /// The platform ran the flow to the end. The sheet may still never have appeared - both
        /// stores keep a per-user quota and stay silent when it is spent - so this means "asked",
        /// not "rated".
        /// </summary>
        Completed,

        /// <summary>
        /// This build has no in-app review path: the Editor, an unsupported platform, or a device
        /// without the store app or its library. The store page is still worth offering.
        /// </summary>
        Unavailable,

        /// <summary>The platform was asked and answered with an error.</summary>
        Failed,

        /// <summary>The platform never answered within the implementation's timeout.</summary>
        TimedOut,

        /// <summary>The caller's token was cancelled before the flow finished.</summary>
        Cancelled,
    }

    /// <summary>
    /// Asks the player to review the game. One implementation per store front - Google Play, the
    /// App Store, and a logging stand-in everywhere else - chosen once in DI, so the game calls the
    /// same two methods on every platform.
    /// <para>
    /// The feature holds no policy: when to ask, how often to ask again, and whether to ask at all
    /// belong to the game that owns the trigger. This service only runs the flow it is handed.
    /// </para>
    /// </summary>
    public interface IRatingService
    {
        /// <summary>
        /// Whether this build has an in-app review path at all (right platform, store library
        /// present). It is a cheap capability check, not a promise that the sheet will appear:
        /// the store decides that per user, and <see cref="RequestReviewAsync"/> is the only way
        /// to find out what happened.
        /// </summary>
        bool IsInAppReviewAvailable { get; }

        /// <summary>
        /// Runs the native in-app review flow and completes once the store is done with it. Never
        /// throws: every failure, refusal, timeout and cancellation comes back as a
        /// <see cref="ReviewFlowResult"/>, because a rating prompt must never break the flow that
        /// opened it.
        /// </summary>
        UniTask<ReviewFlowResult> RequestReviewAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens this build's store listing, where the player can always leave a review. Returns
        /// whether a listing could be opened. This is the fallback the stores themselves recommend
        /// for an explicit "Rate" button, since the in-app sheet may silently decline to show.
        /// </summary>
        bool OpenStorePage();
    }

    public static class RatingServiceExtensions
    {
        /// <summary>
        /// What a Rate button wants: try the native sheet, and fall back to the store listing when
        /// this build has no sheet, or the store errored or went quiet. A cancelled request opens
        /// nothing - the player closed the prompt - and the in-app result is returned either way,
        /// so the caller can still report what was attempted.
        /// </summary>
        public static async UniTask<ReviewFlowResult> RequestReviewOrOpenStoreAsync(
            this IRatingService ratingService,
            CancellationToken   cancellationToken = default
        )
        {
            var result = await ratingService.RequestReviewAsync(cancellationToken);

            if (result is ReviewFlowResult.Completed or ReviewFlowResult.Cancelled) return result;

            ratingService.OpenStorePage();
            return result;
        }
    }
}
