using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.AzureBlobAssetsModule.Core;
using VirtoCommerce.AzureBlobAssetsModule.Core.Extensions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;


namespace VirtoCommerce.AzureBlobAssetsModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public IConfiguration Configuration { get; set; }

        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            var assetsProvider = Configuration.GetSection("Assets:Provider").Value;
            if (assetsProvider.EqualsInvariant(S3BlobProvider.ProviderName))
            {
                serviceCollection.AddOptions<S3BlobOptions>().Bind(Configuration.GetSection("Assets:S3BlobStorage")).ValidateDataAnnotations();
                serviceCollection.AddS3BlobProvider();
            }

        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // Method intentionally left empty
        }

        public void Uninstall()
        {
            // Method intentionally left empty
        }


    }
}
