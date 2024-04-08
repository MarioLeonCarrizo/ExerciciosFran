using System.Windows.Media.Imaging;

namespace Ex1
{
    public static class ResourceImage
    {
        public static BitmapImage GetIcon(string name)
        {
            // Create the resource reader stream.
            var nameSpace = ResourceAssembly.GetNamespace();
            var stream = ResourceAssembly.GetAssembly().GetManifestResourceStream(nameSpace + "RES.Images.Icons." + name);

            var image = new BitmapImage();

            // Construct and return image.
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            // Return constructed BitmapImage.
            return image;
        }
    }
}
