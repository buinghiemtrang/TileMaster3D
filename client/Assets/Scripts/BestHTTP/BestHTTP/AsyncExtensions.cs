using System.Threading;
using System.Threading.Tasks;

namespace BestHTTP
{
	public static class AsyncExtensions
	{
		public static Task<T> GetFromJsonResultAsync<T>(this HTTPRequest request, CancellationToken token = default(CancellationToken))
		{
			return null;
		}
	}
}
