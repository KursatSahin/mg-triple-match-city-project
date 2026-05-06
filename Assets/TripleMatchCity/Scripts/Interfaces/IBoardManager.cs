using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Board
{
    public interface IBoardManager
    {
        IReadOnlyList<CollectibleItemView> ActiveItems { get; }
        UniTask BuildBoard(LevelDataSO level, Transform sceneParent);
        void ClearBoard();
    }
}
