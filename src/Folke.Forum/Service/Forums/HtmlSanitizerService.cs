using System;
using System.IO;
using System.Threading.Tasks;
using Folke.Elm;
using Folke.Elm.Fluent;
using Folke.Forum.Data.Forums;
using Folke.Forum.Infrastructure;

namespace Folke.Forum.Service.Forums
{
    public class HtmlSanitizerService<TUser>
    {
        private readonly IFolkeConnection session;
        private readonly IImageStore imageStore;

        public HtmlSanitizerService(IFolkeConnection session, IImageStore imageStore)
        {
            this.session = session;
            this.imageStore = imageStore;
        }

        public async Task<string> Sanitize(string input, TUser author)
        {
            var html = new HtmlSanitizer(input);
            await SaveImages(html, author);
            return html.Output;
        }

        private async Task SaveImages(HtmlSanitizer html, TUser author)
        {
            foreach (var pair in html.Images)
            {
                var photo = session.SelectAllFrom<Photo<TUser>>().Where(p => p.FileName == pair.Key).SingleOrDefault();
                if (photo == null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        pair.Value.Save(memoryStream);
                        ArraySegment<byte> buffer;
                        if (memoryStream.TryGetBuffer(out buffer))
                        {
                            var url =
                                await imageStore.SaveAsync(buffer.Array, memoryStream.Length, pair.Key);
                            photo = new Photo<TUser>
                            {
                                FileName = pair.Key,
                                Url = url,
                                CreationTime = DateTime.Now,
                                Width = pair.Value.Width,
                                Height = pair.Value.Height,
                                Length = memoryStream.Length,
                                Uploader = author
                            };
                            await session.SaveAsync(photo);
                        }
                    }
                }
            }
        }
    }
}
