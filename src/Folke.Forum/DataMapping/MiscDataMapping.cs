using Folke.Forum.Data.Misc;
using Folke.Forum.Views.Misc;

namespace Folke.Forum.DataMapping
{
    public class MiscDataMapping
    {
        public ExternalLinkView ToExternalLinkDto(ExternalLink externalLink)
        {
            return new ExternalLinkView
            {
                Id = externalLink.Id,
                Title = externalLink.Title,
                Url = externalLink.Url
            };
        }
    }
}
