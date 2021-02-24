using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using XFPhotoGallery.DependencyService;
using XFPhotoGallery.Model;

namespace XFPhotoGallery.PhotoGallery
{
	public class PhotoGalleryCollection : ObservableCollection<MediaAsset>
	{
		readonly IMediaService mediaService;

		int thumbnailQuality = 80;

		public PhotoGalleryCollection()
		{
			mediaService = Xamarin.Forms.DependencyService.Get<IMediaService>();
			mediaService.MediaAssetLoaded += MediaService_MediaAssetLoaded;

			// From Stephen Cleary blog post on async construction: https://blog.stephencleary.com/2013/01/async-oop-2-constructors.html
			Initialization = mediaService.RetrieveMediaAssetsAsync();
		}
		public Task Initialization { get; }

		public int ThumbnailQuality
		{
			get => thumbnailQuality;
			set => SetProperty(ref thumbnailQuality, value);
		}

		private void MediaService_MediaAssetLoaded(object sender, MediaEventArgs e)
		{
			Add(e.Media);
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			switch (e.PropertyName)
			{
				case nameof(ThumbnailQuality):
					mediaService.ThumbnailQuality = ThumbnailQuality;
					break;
			}
		}

		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (Object.Equals(storage, value))
				return false;

			storage = value;
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}
}
