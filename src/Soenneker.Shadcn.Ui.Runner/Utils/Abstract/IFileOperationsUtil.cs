using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Shadcn.Ui.Runner.Utils.Abstract;

public interface IFileOperationsUtil
{
    ValueTask Process(CancellationToken cancellationToken);
}
