namespace GameFoundation.Scripts.Extenstions
{
    using System;
    using System.Threading;
    using Animancer;
    using Cysharp.Threading.Tasks;

    public static class AnimacerExtension
    {
        public static async UniTask ToUniTask(this AnimancerState animancerState, CancellationToken cancellationToken = default)
        {
            await UniTask.WaitUntil(() => animancerState.IsStopped, cancellationToken: cancellationToken);
        }
    }
}