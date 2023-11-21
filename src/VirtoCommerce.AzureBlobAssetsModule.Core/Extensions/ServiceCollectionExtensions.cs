using Microsoft.Extensions.DependencyInjection;
using System;
using VirtoCommerce.Assets.Abstractions;
using VirtoCommerce.AssetsModule.Core.Assets;

namespace VirtoCommerce.AzureBlobAssetsModule.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddS3BlobProvider(this IServiceCollection services, Action<S3BlobOptions> setupAction = null)
        {
            services.AddSingleton<ICommonBlobProvider, S3BlobProvider>();
            services.AddSingleton<IBlobStorageProvider, S3BlobProvider>();
            services.AddSingleton<IBlobUrlResolver, S3BlobProvider>();
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }
        }
    }
}
