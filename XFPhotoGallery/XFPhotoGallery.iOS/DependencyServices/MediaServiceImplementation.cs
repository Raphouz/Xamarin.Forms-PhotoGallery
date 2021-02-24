/* 
 * Original code by TcMarsh31 on GitHub.
 * Taken from the blog post https://medium.com/xamarinlife/image-picker-or-profile-picture-chooser-in-xamarin-forms-dea085ae0adb
 * and the associated GitHub repository https://github.com/TcMarsh31/ImagePicker-in-Xamarin-Forms.
 * Download date: 02/21/2021
 * 
 * Modifications by Raphouz
 * Last modification: 02/23/2021
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CoreGraphics;

using Foundation;

using XFPhotoGallery.DependencyService;
using XFPhotoGallery.iOS.DependencyServiceImplementation;
using XFPhotoGallery.Model;

using Photos;

using UIKit;

using Xamarin.Forms;

using static XFPhotoGallery.Model.MediaAsset;

[assembly: Dependency(typeof(MediaServiceImplementation))]
namespace XFPhotoGallery.iOS.DependencyServiceImplementation
{
	public class MediaServiceImplementation : IMediaService
	{
		public event EventHandler<MediaEventArgs> MediaAssetLoaded;

		public bool IsLoading { get; private set; }

		public int ThumbnailQuality { get; set; }

		CancellationToken cancelToken;

		public MediaServiceImplementation()
		{ }

		public async Task<PHAuthorizationStatus> RequestPermissionAsync()
		{
			var status = PHPhotoLibrary.AuthorizationStatus;

			if (status != PHAuthorizationStatus.Authorized)
			{
				status = await PHPhotoLibrary.RequestAuthorizationAsync();
			}

			return status;

		}

		public async Task<IList<MediaAsset>> RetrieveMediaAssetsAsync(CancellationToken? token = null)
		{
			if (!token.HasValue)
				cancelToken = CancellationToken.None;
			else
				cancelToken = token.Value;

			// We create a TaskCompletionSource of decimal
			var taskCompletionSource = new TaskCompletionSource<IList<MediaAsset>>();

			// Registering a lambda into the cancellationToken
			cancelToken.Register(() =>
			{
				taskCompletionSource.SetCanceled();
			});

			IsLoading = true;

			var task = LoadMediaAsync();

			// Wait for the first task to finish among the two
			var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);
			IsLoading = false;

			return await completedTask;
		}

		async Task<IList<MediaAsset>> LoadMediaAsync()
		{
			IList<MediaAsset> assets = new List<MediaAsset>();

			var imageManager = new PHCachingImageManager();

			var permissionStatus = await RequestPermissionAsync();

			if (permissionStatus == PHAuthorizationStatus.Authorized)
			{
				await Task.Run(() =>
				{
					var thumbnailRequestOptions = new PHImageRequestOptions
					{
						ResizeMode = PHImageRequestOptionsResizeMode.Fast,
						DeliveryMode = PHImageRequestOptionsDeliveryMode.FastFormat,
						NetworkAccessAllowed = true,
						Synchronous = true
					};

					var imageRequestOptions = new PHImageRequestOptions
					{
						ResizeMode = PHImageRequestOptionsResizeMode.Exact,
						DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat,
						NetworkAccessAllowed = true,
						Synchronous = true
					};

					var fetchOptions = new PHFetchOptions
					{
						SortDescriptors = new NSSortDescriptor[] 
						{ 
							new NSSortDescriptor("creationDate", false) 
						},
						Predicate = NSPredicate.FromFormat($"mediaType == {(int)PHAssetMediaType.Image}")
					};

					var fetchResults = PHAsset.FetchAssets(fetchOptions);
					var allAssets = fetchResults.Select(p => p as PHAsset).ToArray();

					var thumbnailSize = new CGSize(256.0f, 256.0f);

					imageManager.StartCaching(allAssets, thumbnailSize, PHImageContentMode.AspectFill, thumbnailRequestOptions);

					var tmpPath = Path.GetTempPath();

					foreach (var result in fetchResults)
					{
						var phAsset = (result as PHAsset);
						var name = PHAssetResource.GetAssetResources(phAsset)?.FirstOrDefault()?.OriginalFilename;
						var asset = new MediaAsset()
						{
							Id = phAsset.LocalIdentifier,
							Name = name,
							Type = MediaAssetType.Image,
						};

						// Requests image on thumbnail size.
						imageManager.RequestImageForAsset(phAsset, thumbnailSize, PHImageContentMode.AspectFit, thumbnailRequestOptions, (image, info) =>
						{
							if (image != null)
							{
								NSData imageData = image.AsJPEG(ThumbnailQuality / 100f);

								if (imageData != null)
								{
									var fileName = Path.Combine(tmpPath, $"tmp_thumb_{name}.jpg");

									imageData.Save(fileName, true, out NSError error);

									if (error == null)
									{
										asset.Thumbnail = fileName;
									}
								}
							}
						});

						if (phAsset.MediaType == PHAssetMediaType.Image)
						{
							// Requests the full image source.
							imageManager.RequestImageForAsset(phAsset, PHImageManager.MaximumSize, PHImageContentMode.AspectFit, imageRequestOptions, (image, info) =>
							{
								if (image != null)
								{
									NSData imageData = null;

									if (image.CGImage.RenderingIntent == CGColorRenderingIntent.Default)
									{
										imageData = image.AsJPEG(1);
									}
									else
									{
										imageData = image.AsPNG();
									}

									if (imageData != null)
									{
										var fileName = Path.Combine(tmpPath, $"tmp_{name}");

										imageData.Save(fileName, true, out NSError error);

										if (error == null)
										{
											asset.Source = fileName;
										}
									}
								}
							});

							UIApplication.SharedApplication.InvokeOnMainThread(() =>
							{
								MediaAssetLoaded?.Invoke(this, new MediaEventArgs(asset));
							});

							assets.Add(asset);
						}

						if (cancelToken.IsCancellationRequested)
							break;
					}
				});

				imageManager.StopCaching();
			}

			return assets;
		}
	}
}
