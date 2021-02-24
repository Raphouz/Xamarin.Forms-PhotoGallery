﻿using Xamarin.Forms;

namespace XFPhotoGallery.Model
{
	public class MediaAsset : Element
    {
        //Image,Video
        public enum MediaAssetType
        {
            Image, Video
        }

        public MediaAssetType Type { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Media { get; set; }
        public string Thumbnail { get; set; }
	}
}