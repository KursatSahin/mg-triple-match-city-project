using Cysharp.Threading.Tasks;

namespace TripleMatch.Core
{
    public interface IDataManager
    {
        bool IsLoaded { get; }
        UniTask LoadData();
        UniTask SaveData();
    }
}