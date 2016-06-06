using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Folke.Elm;
using Folke.Forum.Data.Misc;
using Folke.Forum.DataMapping;
using Folke.Forum.Views.Misc;
using Folke.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Misc
{
    [Route("api/external-link")]
    public class ExternalLinkController : TypedControllerBase
    {
        private readonly IFolkeConnection session;
        private readonly MiscDataMapping miscDataMapping;

        public ExternalLinkController(IFolkeConnection session, MiscDataMapping miscDataMapping)
        {
            this.session = session;
            this.miscDataMapping = miscDataMapping;
        }

        [HttpGet]
        public async Task<IEnumerable<ExternalLinkView>> GetAll()
        {
            return (await session.SelectAllFrom<ExternalLink>().ToListAsync()).Select(l => miscDataMapping.ToExternalLinkDto(l));
        }

        [HttpGet("{id:int}", Name = "GetExternalLink")]
        public async Task<ExternalLinkView> Get(int id)
        {
            return miscDataMapping.ToExternalLinkDto(await session.LoadAsync<ExternalLink>(id));
        }

        [HttpPost]
        [Authorize("ForumAdministrator")]
        public async Task<IHttpActionResult<ExternalLinkView>> Post([FromBody] ExternalLinkView value)
        {
            using (var transaction = session.BeginTransaction())
            {
                var link = new ExternalLink { Title = value.Title, Url = value.Url };
                await session.SaveAsync(link);
                transaction.Commit();
                return Created("GetExternalLink", link.Id, miscDataMapping.ToExternalLinkDto(link));
            }
        }

        [HttpPut("{id:int}")]
        [Authorize("ForumAdministrator")]
        public async Task<IActionResult> Put(int id, [FromBody] ExternalLinkView value)
        {
            using (var transaction = session.BeginTransaction())
            {
                var link = await session.LoadAsync<ExternalLink>(id);
                link.Title = value.Title;
                link.Url = value.Url;
                await session.UpdateAsync(link);
                transaction.Commit();
                return Ok();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var transaction = session.BeginTransaction())
            {
                var link = await session.LoadAsync<ExternalLink>(id);
                await session.DeleteAsync(link);
                transaction.Commit();
                return Ok();
            }
        }
    }
}
