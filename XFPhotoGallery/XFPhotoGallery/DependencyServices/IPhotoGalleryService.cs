using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XFPhotoGallery.PhotoGalleryService
{
	public interface IPhotoGalleryService
	{
		event EventHandler<MediaEventArgs> MediaAssetLoaded;
		Task<IList<MediaAsset>> RetrieveMediaAssetsAsync(CancellationToken? token = null);
		bool IsLoading { get; }

		int ThumbnailQuality { get; set; }
	}

	public class MediaAsset
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Source { get; set; }
		public string Thumbnail { get; set; }
	}

	public class MediaEventArgs
	{
		public MediaAsset Media { get; }
		public MediaEventArgs(MediaAsset media)
		{
			Media = media;
		}
	}
}
