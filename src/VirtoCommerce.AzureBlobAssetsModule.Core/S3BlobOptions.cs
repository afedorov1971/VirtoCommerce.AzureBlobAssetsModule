using System.ComponentModel.DataAnnotations;

namespace VirtoCommerce.AzureBlobAssetsModule.Core
{
    public class S3BlobOptions
    {
        [Required]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Url of the CDN server
        /// </summary>
        public string CdnUrl { get; set; }
    }
}
