using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Folke.Forum.Service
{
    public class FileImageStore : IImageStore
    {
        private readonly FileImageStoreOptions options;

        public FileImageStore(IOptions<FileImageStoreOptions> options)
        {
            this.options = options.Value;
            if (!Directory.Exists(this.options.Path))
                Directory.CreateDirectory(this.options.Path);
        }

        public async Task<string> SaveAsync(byte[] bytes, long size, string fileName)
        {
            var disk = Path.Combine(options.Path, fileName);
            using (var output = File.Create(disk))
            {
                await output.WriteAsync(bytes, 0, (int)size);
            }
            return options.BaseUrl + fileName;
        }

        public Stream Load(string fileName)
        {
            return File.OpenRead(Path.Combine(options.Path, fileName));
        }
    }
}
