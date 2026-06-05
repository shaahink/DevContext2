namespace DevContext.Core.Utilities;

internal static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync2<T>(this IAsyncEnumerable<T> source, CancellationToken ct = default)
    {
        var list = new List<T>();
        await foreach (var item in source.WithCancellation(ct))
        {
            list.Add(item);
        }
        return list;
    }
}
