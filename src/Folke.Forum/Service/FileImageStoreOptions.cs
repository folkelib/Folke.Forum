namespace Folke.Forum.Service
{
    public class FileImageStoreOptions
    {
        /// <summary>Gets or sets the path to the directory where the files are saved</summary>
        public string Path { get; set; }

        /// <summary>Gets or sets the base URL for the images. Must end with a slash.</summary>
        public string BaseUrl { get; set; }
    }
}
