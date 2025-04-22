namespace GameFoundation.Scripts.Extenstions
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;

    public static class TweenExtension
    {
        public static async UniTask ToUniTask(this Tween tween, CancellationToken cancellationToken = default)
        {
            try
            {
                while (tween.IsActive() && tween.IsPlaying())
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                tween.Kill();
                throw;
            }
        }
        
    }
}