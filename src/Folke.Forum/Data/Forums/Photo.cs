using System;
using Folke.Elm;

namespace Folke.Forum.Data.Forums
{
    public class Photo<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public DateTime CreationTime { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long Length { get; set; }
        public Photo<TUser> Original { get; set; }
        public TUser Uploader { get; set; }
    }
}
