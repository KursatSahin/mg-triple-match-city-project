using Cysharp.Threading.Tasks;

namespace TripleMatch.Runtime
{
    /// <summary>
    /// Routes scene transitions in a single place. Loads the requested scene additively,
    /// sets it active, and unloads any other previously loaded scenes.
    /// </summary>
    public interface ISceneFlowService
    {
        UniTask GoToScene(string sceneName);
    }
}
