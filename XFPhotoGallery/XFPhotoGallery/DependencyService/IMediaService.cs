using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XFPhotoGallery.Model;

namespace XFPhotoGallery.DependencyService
{
    public interface IMediaService
    {
        event EventHandler<MediaEventArgs> MediaAssetLoaded;
        Task<IList<MediaAsset>> RetrieveMediaAssetsAsync(CancellationToken? token = null);
        bool IsLoading { get; }

		int ThumbnailQuality { get; set; }
    }
}
