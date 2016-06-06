using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Folke.Elm;
using Folke.Elm.Fluent;
using Folke.Forum.Data.Forums;
using Folke.Forum.DataMapping;
using Folke.Forum.Infrastructure;
using Folke.Forum.Service;
using Folke.Forum.Views.Forums;
using Folke.Mvc.Extensions;
using ImageProcessorCore;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Forums
{
    [Route("api/image")]
    public class ImageController<TUser, TUserView> : TypedControllerBase
    {
        private readonly IFolkeConnection session;
        private readonly IForumUserService<TUser, TUserView> accountService;
        private readonly IImageStore imageStore;
        private readonly ForumsDataMapping<TUser, TUserView> forumsDataMapping;

        public ImageController(IFolkeConnection session, IForumUserService<TUser, TUserView> accountService, IImageStore imageStore, ForumsDataMapping<TUser, TUserView> forumsDataMapping)
        {
            this.session = session;
            this.accountService = accountService;
            this.imageStore = imageStore;
            this.forumsDataMapping = forumsDataMapping;
        }

        [Route("{imageId:int}/{format}")]
        public async Task<IActionResult> Get(int imageId, string format)
        {
            int imageWidth;
            int imageHeight;

            switch (format)
            {
                case "thumb":
                    imageWidth = 256;
                    imageHeight = 256;
                    break;

                default:
                    return BadRequest("Unknown format");
            }

            using (var transaction = session.BeginTransaction())
            {
                var photo = session.SelectAllFrom<Photo<TUser>>().SingleOrDefault(x => x.Original.Id == imageId && x.Width == imageWidth && x.Height == imageHeight);
                if (photo == null)
                {
                    var original = session.Load<Photo<TUser>>(imageId);
                    if (original.Original != null)
                    {
                        return BadRequest();
                    }

                    var fileName = original.FileName.Replace(".", "." + format + ".");
                    using (var originalStream = imageStore.Load(original.FileName))
                    {
                        var fullSizeImage = new  Image(originalStream);
                        var resizedImage = fullSizeImage.ResizeWithRatio(imageWidth, imageHeight);
                        using (var stream = new MemoryStream())
                        {
                            resizedImage.Save(stream);
                            ArraySegment<byte> buffer;
                            if (stream.TryGetBuffer(out buffer))
                            {
                                var url = await imageStore.SaveAsync(buffer.Array, stream.Length, fileName);

                                photo = new Photo<TUser>
                                {
                                    FileName = fileName,
                                    CreationTime = DateTime.UtcNow,
                                    Height = imageHeight,
                                    Width = imageWidth,
                                    Original = original,
                                    Length = stream.Length,
                                    Url = url,
                                    Uploader = original.Uploader
                                };
                                await session.SaveAsync(photo);
                                transaction.Commit();
                            }
                        }
                    }
                }

                return Redirect(photo.Url);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult<PhotoView>> PostFormData()
        {
            var form = Request.Form;

            var files = form.Files["files"];
            
            using (var transaction = session.BeginTransaction())
            {
                var md5 = MD5.Create();
                var bytes = new byte[files.Length];

                using (var stream = files.OpenReadStream())
                {
                    stream.Read(bytes, 0, bytes.Length);
                }

                var baseName = BitConverter.ToString(md5.ComputeHash(bytes));
                var memoryStream = new MemoryStream(bytes);
                var fullSizeImage = new Image(memoryStream);

                var imageFormat = fullSizeImage.CurrentImageFormat;
                var extension = imageFormat.Encoder.Extension;
                
                var localFileName = baseName + "." + extension;

                var photo = await session.SelectAllFrom<Photo<TUser>>().Where(i => i.FileName == localFileName).FirstOrDefaultAsync();
                if (photo == null)
                {
                    using (var stream = new MemoryStream())
                    {
                        fullSizeImage.Save(stream);
                        ArraySegment<byte> buffer;
                        if (stream.TryGetBuffer(out buffer))
                        {
                            var url = await imageStore.SaveAsync(buffer.Array, stream.Length, localFileName);
                            photo = new Photo<TUser>
                            {
                                CreationTime = DateTime.UtcNow,
                                FileName = localFileName,
                                Width = fullSizeImage.Width,
                                Height = fullSizeImage.Height,
                                Length = stream.Length,
                                Url = url,
                                Uploader = await accountService.GetCurrentUserAsync()
                            };
                            await session.SaveAsync(photo);
                        }
                    }
                }

                transaction.Commit();
                return Ok(forumsDataMapping.ToPhotoView(photo));
            }
        }
	}
}