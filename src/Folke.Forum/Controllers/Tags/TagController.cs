using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Folke.Elm;
using Folke.Elm.Fluent;
using Folke.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Tags
{
    [Route("api/tag")]
    public class TagController : TypedControllerBase
    {
        private readonly IFolkeConnection session;

        public TagController(IFolkeConnection session)
        {
            this.session = session;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Complete([FromQuery] string query)
        {
            var pattern = query.Replace("%", "") + "%";
            return (await session.SelectAllFrom<Data.Tags.Tag>().Where(n => n.Text.Like(pattern)).OrderBy(n => n.Text).Asc().Limit(0, 10).ToListAsync())
                .Select(n => n.Text);
        }
    }
}
