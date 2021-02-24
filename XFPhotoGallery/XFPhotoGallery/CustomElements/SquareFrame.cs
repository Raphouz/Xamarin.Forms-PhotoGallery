using Xamarin.Forms;

namespace XFPhotoGallery.CustomElements
{
	public class SquareFrame : Frame
	{
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(widthConstraint, widthConstraint));
			//return base.OnMeasure(widthConstraint, widthConstraint);
		}
	}
}
