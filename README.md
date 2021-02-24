# Xamarin.Forms-PhotoGallery
Photo gallery sample for Xamarin.Forms

Application sample for displaying photo gallery using images saved to the device external storage.
This work is based on the [ImagePicker-in-Xamarin-Forms](https://github.com/TcMarsh31/ImagePicker-in-Xamarin-Forms) repository by [TcMarsh31](https://github.com/TcMarsh31).

### The IMediaService interface
This sample uses a dependency service of type `IMediaService` to acquire the photos stored on the device.
Each platform (only Android and iOS are supported at the moment) has its own implementation.

The `IMediaService.RetrieveMediaAssetsAsync()` method return a list of `MediaAsset` objects, that can be used to get a thumbnail or the source image path.

### The PhotoGalleryCollection class
This class extends from `ObservableCollection<MediaAsset>` and allows the photo gallery to be used as a source collection within any control, with minimal code.
The example below shows how to display the loaded images in a `CollectionView`.

```xaml
<CollectionView x:Name="gallery"
                SelectionMode="Single"
                ItemSizingStrategy="MeasureAllItems">

    <CollectionView.ItemsSource>
        <gallery:PhotoGalleryCollection ThumbnailQuality="70"/>
    </CollectionView.ItemsSource>
			
    <CollectionView.ItemsLayout>
        <GridItemsLayout Orientation="Vertical"
                         Span="3"/>
    </CollectionView.ItemsLayout>

    <CollectionView.ItemTemplate>
        <DataTemplate>
            <custom:SquareFrame Padding="0"
                                BorderColor="White">
                <Image Source="{Binding Thumbnail}"
                       Aspect="AspectFill"/>
            </custom:SquareFrame>
        </DataTemplate>
    </CollectionView.ItemTemplate>
			
</CollectionView>
```
