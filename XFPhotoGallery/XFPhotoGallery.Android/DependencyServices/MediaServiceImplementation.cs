/* 
 * Original code by TcMarsh31 on GitHub.
 * Taken from the blog post https://medium.com/xamarinlife/image-picker-or-profile-picture-chooser-in-xamarin-forms-dea085ae0adb
 * and the associated GitHub repository https://github.com/TcMarsh31/ImagePicker-in-Xamarin-Forms.
 * Download date: 02/21/2021
 * 
 * Modifications by Raphouz
 * Last modification: 02/22/2021
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Android.Graphics;
using Android.OS;
using Android.Provider;

using XFPhotoGallery.DependencyService;
using XFPhotoGallery.Droid.DependencyServiceImplementation;
using XFPhotoGallery.Model;

using Xamarin.Essentials;
using Xamarin.Forms;

using static XFPhotoGallery.Model.MediaAsset;

[assembly: Dependency(typeof(MediaServiceImplementation))]
namespace XFPhotoGallery.Droid.DependencyServiceImplementation
{
	public class MediaServiceImplementation : IMediaService
	{
		public event EventHandler<MediaEventArgs> MediaAssetLoaded;

		public bool IsLoading { get; private set; }

		public int ThumbnailQuality { get; set; } = 100;

		CancellationToken cancelToken;

		public MediaServiceImplementation()
		{ }

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
				// We received a cancellation message, cancel the TaskCompletionSource.Task
				taskCompletionSource.TrySetCanceled();
			});

			IsLoading = true;

			var task = LoadMediaAsync();

			// Wait for the first task to finish among the two
			var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);
			IsLoading = false;

			return await completedTask;
		}

		private async Task<IList<MediaAsset>> LoadMediaAsync()
		{
			IList<MediaAsset> assets = new List<MediaAsset>();

			var hasPermission = await CheckAndRequestStorageReadPermission();

			if (hasPermission == PermissionStatus.Granted)
			{
				var uri = MediaStore.Files.GetContentUri("external");
				var ctx = Android.App.Application.Context;

				await Task.Run(() =>
				{
					var cursor = ctx.ContentResolver.Query(uri, new string[]
					{
						MediaStore.Files.FileColumns.Id,
						MediaStore.Files.FileColumns.Data,
						MediaStore.Files.FileColumns.MediaType,
						MediaStore.Files.FileColumns.DisplayName,
						MediaStore.Files.FileColumns.Size
					//}, $"{MediaStore.MediaStore.Files.FileColumns.MediaType} = {(int)MediaType.Image} OR {MediaStore.MediaStore.Files.FileColumns.MediaType} = {(int)MediaType.Video}", null, $"{MediaStore.MediaStore.Files.FileColumns.DateAdded} DESC");
					}, 
					$"{MediaStore.Files.FileColumns.MediaType} = {(int)MediaType.Image}",
					null,
					$"{MediaStore.Files.FileColumns.DateAdded} DESC");

					if (cursor.Count > 0)
					{
						while (cursor.MoveToNext())
						{
							try
							{
								var id = cursor.GetInt(cursor.GetColumnIndex(MediaStore.Files.FileColumns.Id));
								var name = cursor.GetString(cursor.GetColumnIndex(MediaStore.Files.FileColumns.DisplayName));
								var path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Files.FileColumns.Data));
								var mediaType = (MediaType)cursor.GetInt(cursor.GetColumnIndex(MediaStore.Files.FileColumns.MediaType));

								if (mediaType == MediaType.Image)
								{
									// Creates an in-memory thumbnail for the image.
									var thumbnailPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"tmp_thumb_{id}");

									Bitmap thumbnail = MediaStore.Images.Thumbnails.GetThumbnail(ctx.ContentResolver, id, ThumbnailKind.MiniKind, new BitmapFactory.Options()
									{
										InSampleSize = 1,
										InPurgeable = true
									});

									using (var stream = new FileStream(thumbnailPath, FileMode.Create))
									{
										thumbnail.Compress(Bitmap.CompressFormat.Jpeg, ThumbnailQuality, stream);	// The quality parameter is ignored for Png formats
										stream.Close();
									}

									var asset = new MediaAsset()
									{
										Id = id.ToString(),
										Name = name,
										Source = path,
										Thumbnail = thumbnailPath,
										Type = MediaAssetType.Image,
									};

									using (var handler = new Handler(Looper.MainLooper))
									{
										handler.Post(() =>
										{
											MediaAssetLoaded?.Invoke(this, new MediaEventArgs(asset));
										});
									}

									assets.Add(asset);
								}

								if (cancelToken.IsCancellationRequested)
									break;
							}
							catch (Exception)
							{ 
								// TODO: display a default image ?
							}
						}
					}
				});
			}
			return assets;
		}

		public async Task<PermissionStatus> CheckAndRequestStorageReadPermission()
		{
			var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

			if (status == PermissionStatus.Granted)
				return status;

			status = await Permissions.RequestAsync<Permissions.StorageRead>();

			return status;
		}
	}
}

