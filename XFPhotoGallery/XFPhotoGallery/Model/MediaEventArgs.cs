using System;
namespace XFPhotoGallery.Model
{
    public class MediaEventArgs
    {
        public MediaAsset Media { get; }
        public MediaEventArgs(MediaAsset media)
        {
            Media = media;
        }
    }
}
