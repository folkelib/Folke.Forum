using System.IO;
using System.Threading.Tasks;

namespace Folke.Forum.Service
{
    public interface IImageStore
    {
        Task<string> SaveAsync(byte[] stream, long size, string fileName);
        Stream Load(string fileName);
    }
}
