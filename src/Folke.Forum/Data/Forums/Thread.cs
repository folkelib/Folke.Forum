using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Folke.Elm;
using Folke.Elm.Mapping;
using Folke.Forum.Data.Comments;
using Folke.Forum.Data.Tags;

namespace Folke.Forum.Data.Forums
{
    public class Thread<TUser>: ICommentable, IFolkeTable
    {
        public int Id { get; set; }
        [Index(Name= "ThreadCreationDate")]
        public DateTime CreationDate { get; set; }
        public TUser Author { get; set; }
        [Index(Name = "ThreadForum")]
        public Forum Forum { get; set; }
        public string Title { get; set; }
        [MaxLength(65536)]
        public string Text { get; set; }
        public bool Sticky { get; set; }
        public int NumberOfComments { get; set; }
        public ICollection<ThreadLastViewed<TUser>> LastViewed { get; set; }
        [Select(IncludeReference = nameof(PhotoInThread<TUser>.Photo))]
        public IReadOnlyList<PhotoInThread<TUser>> Photos { get; set; }
        [Select(IncludeReference = nameof(TagThread<TUser>.Tag))]
        public IReadOnlyList<TagThread<TUser>> Tags { get; set; }
    }
}
