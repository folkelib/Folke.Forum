using ImageProcessorCore;
using ImageProcessorCore.Samplers;

namespace Folke.Forum.Infrastructure
{
    public static class ImageHelper
    {
        public static Image ResizeWithRatio(this Image image, int width, int height)
        {
            if (width == 0)
                width = image.Width;
            if (height == 0)
                height = width * image.Height / image.Width;

            var newImage = new Image(image);
            return newImage.Resize(width, height);
        }
    }
}
