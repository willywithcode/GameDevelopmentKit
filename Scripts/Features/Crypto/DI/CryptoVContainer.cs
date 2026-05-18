namespace GameFoundation.Scripts.Features.Crypto.DI
{
    using GameFoundation.Scripts.Features.Crypto.Services;
    using VContainer;

    public static class CryptoVContainer
    {
        public static void RegisterCrypto(this IContainerBuilder builder)
        {
            builder.Register<CryptoService>(Lifetime.Singleton)
                   .AsSelf()
                   .AsImplementedInterfaces();
        }
    }
}
