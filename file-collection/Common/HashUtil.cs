namespace XDFileCollection.Common;

#region ReSharper disable

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

internal static class HashUtil
{
    public const string SeedUuid = "873AC3C2-BFCB-4DE0-A463-B023440ABC06";
    public const int HashFileBlockSize = 2048 * 1024;  /*2 MiB*/

    private static readonly ulong Seed = WyHash.WyHash64.ComputeHash64(new Guid(SeedUuid).ToByteArray());

    /// <summary>
    /// 对文件分块取哈希
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static ulong[] Hash(in string path)
    {
        using var f = File.Open(path, FileMode.Open);
        var block = new byte[HashFileBlockSize];
        var offset = 0L;
        var leftSize = f.Length;
        var size = (long)Math.Ceiling((double)f.Length / HashFileBlockSize);
        if (size <= 0) return Array.Empty<ulong>();

        var hashList = new ulong[size];
        var idx = 0;
        while (leftSize > 0)
        {
            if (leftSize <= HashFileBlockSize)
            {
                Array.Clear(block);
                var _ = f.Read(block, (int)offset, (int)leftSize);
                leftSize = 0;
            }
            else
            {
                var _ = f.Read(block, (int)offset, HashFileBlockSize);
                leftSize -= HashFileBlockSize;
                offset += HashFileBlockSize;
            }
            hashList[idx++] = WyHash.WyHash64.ComputeHash64(block, Seed);
        }
        return hashList;
    }

    /// <summary>
    /// 对文件分块取哈希
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static ulong[] Hash(in FileStream f)
    {
        f.Seek(0, SeekOrigin.Begin);
        var block = new byte[HashFileBlockSize];
        var offset = 0L;
        var leftSize = f.Length;
        var size = (long)Math.Ceiling((double)f.Length / HashFileBlockSize);
        if (size <= 0) return Array.Empty<ulong>();

        var hashList = new ulong[size];
        var idx = 0;
        while (leftSize > 0)
        {
            if (leftSize <= HashFileBlockSize)
            {
                Array.Clear(block);
                var _ = f.Read(block, (int)offset, (int)leftSize);
                leftSize = 0;
            }
            else
            {
                var _ = f.Read(block, (int)offset, HashFileBlockSize);
                leftSize -= HashFileBlockSize;
                offset += HashFileBlockSize;
            }
            hashList[idx++] = WyHash.WyHash64.ComputeHash64(block, Seed);
        }
        return hashList;
    }

    /// <summary>
    /// 比较两个哈希数组是否相同
    /// </summary>
    /// <param name="hashA"></param>
    /// <param name="hashB"></param>
    /// <returns></returns>
    public static bool CompareHash(in IReadOnlyList<ulong> hashA, in IReadOnlyList<ulong> hashB)
    {
        if (hashA.Count != hashB.Count) return false;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < hashA.Count; i++)
            if (hashA[i] != hashB[i]) return false;
        return true;
    }
}