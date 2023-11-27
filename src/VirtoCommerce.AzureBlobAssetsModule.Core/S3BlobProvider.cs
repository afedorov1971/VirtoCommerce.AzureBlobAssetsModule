using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using VirtoCommerce.Assets.Abstractions;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Extensions;
using VirtoCommerce.Platform.Core.Settings;
using BlobInfo = VirtoCommerce.AssetsModule.Core.Assets.BlobInfo;

namespace VirtoCommerce.MinIoAssetsModule.Core
{
    public class S3BlobProvider : BasicBlobProvider, IBlobStorageProvider, IBlobUrlResolver, ICommonBlobProvider
    {
        public const string ProviderName = "MinIoStorage";
        public const string BlobCacheControlPropertyValue = "public, max-age=604800";
        private const string Delimiter = "/";
        private readonly MinioClient _blobServiceClient;
        private readonly Uri _baseUri;

        public S3BlobProvider(IOptions<S3BlobOptions> options, IOptions<PlatformOptions> platformOptions, ISettingsManager settingsManager) : base(platformOptions, settingsManager)
        {
            _blobServiceClient = new MinioClient().WithEndpoint("localhost:9001")
                .WithCredentials("admin", "password")
                .Build();

            _baseUri = new Uri("http://localhost:9091");
        }

        #region ICommonBlobProvider members

        public bool Exists(string blobUrl)
        {
            return ExistsAsync(blobUrl).GetAwaiter().GetResult();
        }

        public async Task<bool> ExistsAsync(string blobUrl)
        {
            var bucketName = string.Empty;
            string objectName;

            var pos = blobUrl.LastIndexOf('/');
            if (pos >= 0)
            {
                objectName = blobUrl.Substring(pos + 1);
                bucketName = blobUrl.Substring(0, pos);
            }
            else
            {
                objectName = blobUrl;
            }

            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);
                var result = await _blobServiceClient.StatObjectAsync(statObjectArgs).ConfigureAwait(false);
                return result != null;
            }
            catch (MinioException me)
            {
                Console.WriteLine($"[Bucket] IAMAWSProviderExample example case encountered Exception: {me}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Bucket] IAMAWSProviderExample example case encountered Exception: {e}");
            }

            return false;
        }

        #endregion ICommonBlobProvider members

        #region IBlobStorageProvider Members

        /// <summary>
        /// Get blob info by URL
        /// </summary>
        /// <param name="blobUrl">Absolute or relative URL to get blob</param>
        public virtual async Task<BlobInfo> GetBlobInfoAsync(string blobUrl)
        {
            throw new NotImplementedException();
            //if (string.IsNullOrEmpty(blobUrl))
            //{
            //    throw new ArgumentNullException(nameof(blobUrl));
            //}

            //var uri = blobUrl.IsAbsoluteUrl() ? new Uri(blobUrl) : new Uri(_baseUri, blobUrl.TrimStart(Delimiter[0]));
            //BlobInfo result = null;
            //try
            //{
            //    var blob = new BlobClient(new Uri(_blobServiceClient.Uri, uri.AbsolutePath.TrimStart('/')));
            //    var props = await blob.GetPropertiesAsync();
            //    result = ConvertBlobToBlobInfo(blob, props.Value);
            //}
            //catch
            //{
            //    // Since the storage account is based on transaction volume, it is better to handle the 404 (BlobNotFound) exception because that is just one api call, as opposed to checking the BlobClient.ExistsAsync() first and then making the BlobClient.DownloadAsync() call (2 api transactions).
            //    //https://elcamino.cloud/articles/2020-03-30-azure-storage-blobs-net-sdk-v12-upgrade-guide-and-tips.html
            //}

            //return result;
        }

        /// <summary>
        /// Open stream for read blob by relative or absolute url
        /// </summary>
        /// <param name="blobUrl"></param>
        /// <returns>blob stream</returns>
        public virtual Stream OpenRead(string blobUrl)
        {
            return OpenReadAsync(blobUrl).GetAwaiter().GetResult();
        }

        public virtual Task<Stream> OpenReadAsync(string blobUrl)
        {
            if (string.IsNullOrEmpty(blobUrl))
            {
                throw new ArgumentNullException(nameof(blobUrl));
            }

            throw new NotImplementedException();

            //var container = _blobServiceClient.GetBlobContainerClient(GetContainerNameFromUrl(blobUrl));
            //var blob = container.GetBlockBlobClient(GetFilePathFromUrl(blobUrl));

            //return blob.OpenReadAsync();
        }

        /// <summary>
        /// Open blob for write by relative or absolute url
        /// </summary>
        /// <param name="blobUrl"></param>
        /// <returns>blob stream</returns>
        public virtual Stream OpenWrite(string blobUrl)
        {
            return OpenWriteAsync(blobUrl).GetAwaiter().GetResult();
        }

        public virtual async Task<Stream> OpenWriteAsync(string blobUrl)
        {
            var filePath = GetFilePathFromUrl(blobUrl);

            if (filePath == null)
            {
                throw new ArgumentException(@"Cannot get file path from URL", nameof(blobUrl));
            }

            if (IsExtensionBlacklisted(filePath))
            {
                throw new PlatformException($"This extension is not allowed. Please contact administrator.");
            }

            throw new NotImplementedException();

            //var container = _blobServiceClient.GetBlobContainerClient(GetContainerNameFromUrl(blobUrl));
            //await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            //var blob = container.GetBlockBlobClient(filePath);

            //var options = new BlockBlobOpenWriteOptions
            //{
            //    HttpHeaders = new BlobHttpHeaders
            //    {
            //        // Use HTTP response headers to instruct the browser regarding the safe use of uploaded files, when downloaded from the system
            //        ContentType = MimeTypeResolver.ResolveContentType(filePath),
            //        // Leverage Browser Caching - 7days
            //        // Setting Cache-Control on Azure Blobs can help reduce bandwidth and improve the performance by preventing consumers from having to continuously download resources.
            //        // More Info https://developers.google.com/speed/docs/insights/LeverageBrowserCaching
            //        CacheControl = BlobCacheControlPropertyValue
            //    }
            //};

            //// FlushLessStream wraps BlockBlobWriteStream to not use Flush multiple times.
            //// !!! Call Flush several times on a plain BlockBlobWriteStream causes stream hangs/errors.
            //// https://github.com/Azure/azure-sdk-for-net/issues/20652
            //return new FlushLessStream(await blob.OpenWriteAsync(true, options));
        }

        public virtual async Task RemoveAsync(string[] urls)
        {
            throw new NotImplementedException();
            //foreach (var url in urls.Where(x => !string.IsNullOrWhiteSpace(x)))
            //{
            //    var absoluteUri = url.IsAbsoluteUrl()
            //        ? new Uri(url).ToString()
            //        : UrlHelperExtensions.Combine(_blobServiceClient.Uri.ToString(), url);
            //    var blobContainer = GetBlobContainer(GetContainerNameFromUrl(absoluteUri));

            //    var isFolder = string.IsNullOrEmpty(Path.GetFileName(absoluteUri));
            //    var blobSearchPrefix = isFolder ? GetDirectoryPathFromUrl(absoluteUri)
            //                                 : GetFilePathFromUrl(absoluteUri);

            //    if (string.IsNullOrEmpty(blobSearchPrefix))
            //    {
            //        await blobContainer.DeleteIfExistsAsync();
            //    }
            //    else
            //    {
            //        var blobItems = blobContainer.GetBlobsAsync(prefix: blobSearchPrefix);
            //        await foreach (var blobItem in blobItems)
            //        {
            //            var blobClient = blobContainer.GetBlobClient(blobItem.Name);
            //            await blobClient.DeleteIfExistsAsync();
            //        }
            //    }
            //}
        }

        public virtual async Task<BlobEntrySearchResult> SearchAsync(string folderUrl, string keyword)
        {
            var result = AbstractTypeFactory<BlobEntrySearchResult>.TryCreateInstance();

            if (!string.IsNullOrEmpty(folderUrl))
            {
                throw new NotImplementedException();

                //    var container = GetBlobContainer(GetContainerNameFromUrl(folderUrl));

                //    if (container != null)
                //    {
                //        var baseUriEscaped = container.Uri.AbsoluteUri; // absoluteUri is escaped already
                //        var prefix = GetDirectoryPathFromUrl(folderUrl);
                //        if (!string.IsNullOrEmpty(keyword))
                //        {
                //            //Only whole container list allow search by prefix
                //            prefix += keyword;
                //        }

                //        var containerProperties = await container.GetPropertiesAsync();

                //        // Call the listing operation and return pages of the specified size.
                //        var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: Delimiter)
                //            .AsPages();

                //        // Enumerate the blobs returned for each page.
                //        await foreach (var blobPage in resultSegment)
                //        {
                //            // A hierarchical listing may return both virtual directories and blobs.
                //            foreach (var blobHierarchyItem in blobPage.Values)
                //            {
                //                if (blobHierarchyItem.IsPrefix)
                //                {
                //                    var folder = AbstractTypeFactory<BlobFolder>.TryCreateInstance();

                //                    folder.Name = GetOutlineFromUrl(blobHierarchyItem.Prefix).Last();
                //                    folder.Url = UrlHelperExtensions.Combine(baseUriEscaped, EscapeUri(blobHierarchyItem.Prefix));
                //                    folder.ParentUrl = GetParentUrl(baseUriEscaped, blobHierarchyItem.Prefix);
                //                    folder.RelativeUrl = folder.Url.Replace(_baseUri.AbsoluteUri.TrimEnd(Delimiter[0]), string.Empty);
                //                    folder.CreatedDate = containerProperties.Value.LastModified.UtcDateTime;
                //                    folder.ModifiedDate = containerProperties.Value.LastModified.UtcDateTime;
                //                    result.Results.Add(folder);
                //                }
                //                else
                //                {
                //                    var blobInfo = ConvertBlobToBlobInfo(blobHierarchyItem.Blob, baseUriEscaped);
                //                    //Do not return empty blob (created with directory because azure blob not support direct directory creation)
                //                    if (!string.IsNullOrEmpty(blobInfo.Name))
                //                    {
                //                        result.Results.Add(blobInfo);
                //                    }
                //                }
                //            }
                //        }
                //    }
            }
            else
            {
                // Call the listing operation and enumerate the result segment.
                var resultSegment = await _blobServiceClient.ListBucketsAsync();
                
                foreach (var bucket in resultSegment.Buckets)
                {
                    var folder = AbstractTypeFactory<BlobFolder>.TryCreateInstance();
                    folder.Name = bucket.Name.Split(Delimiter).Last();
                    folder.Url = EscapeUri(UrlHelperExtensions.Combine(_baseUri.ToString(), bucket.Name));

                    result.Results.Add(folder);
                }
            }

            result.TotalCount = result.Results.Count;
            return result;
        }

        public virtual async Task CreateFolderAsync(BlobFolder folder)
        {
            throw new NotImplementedException();
            //var path = folder.ParentUrl == null ?
            //            folder.Name :
            //            UrlHelperExtensions.Combine(folder.ParentUrl, folder.Name);

            //var containerName = GetContainerNameFromUrl(path);
            //var container = _blobServiceClient.GetBlobContainerClient(containerName);
            //await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            //var directoryPath = GetDirectoryPathFromUrl(path);
            //if (!string.IsNullOrEmpty(directoryPath))
            //{
            //    //Need upload empty blob because azure blob storage not support direct directory creation
            //    using var stream = new MemoryStream(new byte[0]);
            //    var keepFile = string.Join(Delimiter, directoryPath.TrimEnd(Delimiter[0]), ".keep");
            //    await container.GetBlockBlobClient(keepFile).UploadAsync(stream);
            //}
        }

        public virtual void Move(string srcUrl, string destUrl)
        {
            MoveAsync(srcUrl, destUrl).GetAwaiter().GetResult();
        }

        public virtual Task MoveAsyncPublic(string srcUrl, string destUrl)
        {
            return MoveAsync(srcUrl, destUrl);
        }

        public virtual void Copy(string srcUrl, string destUrl)
        {
            MoveAsync(srcUrl, destUrl, true).GetAwaiter().GetResult();
        }

        public virtual Task CopyAsync(string srcUrl, string destUrl)
        {
            return MoveAsync(srcUrl, destUrl, true);
        }

        protected virtual async Task MoveAsync(string oldUrl, string newUrl, bool isCopy = false)
        {
            throw new NotImplementedException();
            //string oldPath;
            //string newPath;
            //var isFolderRename = string.IsNullOrEmpty(Path.GetFileName(oldUrl));

            //var containerName = GetContainerNameFromUrl(oldUrl);

            ////if rename file
            //if (!isFolderRename)
            //{
            //    oldPath = GetFilePathFromUrl(oldUrl);
            //    newPath = GetFilePathFromUrl(newUrl);
            //}
            //else
            //{
            //    oldPath = GetDirectoryPathFromUrl(oldUrl);
            //    newPath = GetDirectoryPathFromUrl(newUrl);
            //}

            //var taskList = new List<Task>();
            //var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
            //var blobItems = blobContainer.GetBlobsAsync(prefix: oldPath);

            //await foreach (var blobItem in blobItems)
            //{
            //    var blobName = UrlHelperExtensions.Combine(containerName, blobItem.Name);
            //    var newBlobName = blobName.Replace(oldPath, newPath);

            //    taskList.Add(MoveBlob(blobContainer, blobName, newBlobName, isCopy));
            //}

            //await Task.WhenAll(taskList);
        }

        ///// <summary>
        ///// Move blob new URL and remove old blob
        ///// </summary>
        ///// <param name="container"></param>
        ///// <param name="oldUrl"></param>
        ///// <param name="newUrl"></param>
        ///// <param name="isCopy"></param>
        //private async Task MoveBlob(BlobContainerClient container, string oldUrl, string newUrl, bool isCopy)
        //{
        //    var targetPath = newUrl.EndsWith(Delimiter)
        //        ? GetDirectoryPathFromUrl(newUrl)
        //        : GetFilePathFromUrl(newUrl);

        //    if (IsExtensionBlacklisted(targetPath))
        //    {
        //        throw new PlatformException($"This extension is not allowed. Please contact administrator.");
        //    }

        //    var target = container.GetBlockBlobClient(targetPath);

        //    if (!await target.ExistsAsync())
        //    {
        //        var soursePath = oldUrl.EndsWith(Delimiter)
        //            ? GetDirectoryPathFromUrl(oldUrl)
        //            : GetFilePathFromUrl(oldUrl);

        //        var sourceBlob = container.GetBlockBlobClient(soursePath);

        //        if (await sourceBlob.ExistsAsync())
        //        {
        //            await target.StartCopyFromUri(sourceBlob.Uri).WaitForCompletionAsync();

        //            if (!isCopy)
        //            {
        //                await sourceBlob.DeleteIfExistsAsync();
        //            }
        //        }
        //    }
        //}

        #endregion IBlobStorageProvider Members

        #region IBlobUrlResolver Members

        public string GetAbsoluteUrl(string blobKey)
        {
            var result = blobKey;
            if (!blobKey.IsAbsoluteUrl())
            {
                var baseUrl = _baseUri.AbsoluteUri;

                result = UrlHelperExtensions.Combine(baseUrl, EscapeUri(blobKey));
            }

            return result;
        }

        #endregion IBlobUrlResolver Members

        /// <summary>
        /// Return outline folder from absolute or relative URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string[] GetOutlineFromUrl(string url)
        {
            var relativeUrl = url;
            if (url.IsAbsoluteUrl())
            {
                relativeUrl = Uri.UnescapeDataString(new Uri(url).AbsolutePath);
            }

            var start = 0;
            var end = 0;
            if (relativeUrl.StartsWith(Delimiter))
            {
                start++;
            }
            if (relativeUrl.EndsWith(Delimiter))
            {
                end++;
            }
            relativeUrl = relativeUrl[start..^end];

            return relativeUrl.Split(Delimiter[0], '\\'); // name may be empty
        }

        private string GetContainerNameFromUrl(string url)
        {
            return GetOutlineFromUrl(url).First();
        }

        private string GetDirectoryPathFromUrl(string url)
        {
            var result = string.Join(Delimiter, GetOutlineFromUrl(url).Skip(1).ToArray());
            return !string.IsNullOrEmpty(result) ? HttpUtility.UrlDecode(result) + Delimiter : null;
        }

        private string GetFilePathFromUrl(string url)
        {
            var result = string.Join(Delimiter, GetOutlineFromUrl(url).Skip(1).ToArray());
            return !string.IsNullOrEmpty(result) ? HttpUtility.UrlDecode(result) : null;
        }

        private string GetParentUrl(string baseUri, string blobPrefix)
        {
            var segments = GetOutlineFromUrl(blobPrefix);
            var parentPath = string.Join(Delimiter, segments.Take(segments.Length - 1));
            return UrlHelperExtensions.Combine(baseUri, EscapeUri(parentPath));
        }

        private static string EscapeUri(string stringToEscape)
        {
            var uri = new Uri(stringToEscape, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                return uri.AbsoluteUri;
            }

            var parts = stringToEscape.Split(new[] { Delimiter[0] });
            return string.Join(Delimiter, parts.Select(Uri.EscapeDataString));
        }

        //private BlobContainerClient GetBlobContainer(string name)
        //{
        //    BlobContainerClient result = null;
        //    // Retrieve container reference.
        //    var container = _blobServiceClient.GetBlobContainerClient(name);
        //    if (container.Exists())
        //    {
        //        result = container;
        //    }

        //    return result;
        //}

        private BlobInfo ConvertBlobToBlobInfo(BlobClient blob, BlobProperties props)
        {
            var absoluteUrl = blob.Uri;
            var relativeUrl = Delimiter + UrlHelperExtensions.Combine(GetContainerNameFromUrl(blob.Uri.AbsoluteUri), EscapeUri(blob.Name));
            var fileName = Path.GetFileName(Uri.UnescapeDataString(blob.Name));
            var contentType = MimeTypeResolver.ResolveContentType(fileName);

            return new BlobInfo
            {
                Url = absoluteUrl.AbsoluteUri,
                Name = fileName,
                ContentType = contentType,
                Size = props.ContentLength,
                CreatedDate = props.CreatedOn.UtcDateTime,
                ModifiedDate = props.LastModified.UtcDateTime,
                RelativeUrl = relativeUrl
            };
        }

        //private BlobInfo ConvertBlobToBlobInfo(BlobItem blob, string baseUri)
        //{
        //    var fileName = Path.GetFileName(blob.Name);
        //    var absoluteUrl = UrlHelperExtensions.Combine(baseUri, EscapeUri(blob.Name));
        //    var relativeUrl = Delimiter + absoluteUrl.Replace(EscapeUri(_blobServiceClient.Uri.ToString()), string.Empty);
        //    var contentType = MimeTypeResolver.ResolveContentType(fileName);

        //    return new BlobInfo
        //    {
        //        Url = absoluteUrl,
        //        Name = fileName,
        //        ContentType = contentType,
        //        Size = blob.Properties.ContentLength ?? 0,
        //        CreatedDate = blob.Properties.CreatedOn.Value.UtcDateTime,
        //        ModifiedDate = blob.Properties.LastModified?.UtcDateTime,
        //        RelativeUrl = relativeUrl
        //    };
        //}
    }
}
