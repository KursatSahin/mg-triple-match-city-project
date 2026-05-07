using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace TripleMatch.UI
{
    /// <summary>
    /// IUIService implementation. Resolves screens through VContainer's IObjectResolver and
    /// keeps a stack of currently open screens. Screens must be registered in the container
    /// (typically via RegisterComponent for MonoBehaviour-backed views).
    /// </summary>
    public sealed class UIService : IUIService
    {
        private readonly IObjectResolver _container;
        private readonly Stack<IScreen> _stack = new();
        private readonly Dictionary<Type, IScreen> _openByType = new();

        public IScreen Top => _stack.Count > 0 ? _stack.Peek() : null;

        public UIService(IObjectResolver container)
        {
            _container = container;
        }

        public async UniTask<TScreen> Open<TScreen, TArgs>(TArgs args) where TScreen : class, IScreen<TArgs>
        {
            TScreen screen = _container.Resolve<TScreen>();
            if (screen == null)
            {
                Debug.LogError($"[UIService] Cannot resolve screen type {typeof(TScreen).Name}.");
                return null;
            }

            if (_openByType.ContainsKey(typeof(TScreen)))
            {
                Debug.LogWarning($"[UIService] Screen {typeof(TScreen).Name} is already open. Ignoring duplicate Open.");
                return screen;
            }

            await screen.OnOpenAsync(args);

            _stack.Push(screen);
            _openByType[typeof(TScreen)] = screen;
            return screen;
        }

        public async UniTask CloseTop()
        {
            if (_stack.Count == 0) return;

            IScreen top = _stack.Pop();
            _openByType.Remove(top.GetType());
            await top.OnCloseAsync();
        }

        public async UniTask CloseAll()
        {
            while (_stack.Count > 0)
            {
                IScreen top = _stack.Pop();
                _openByType.Remove(top.GetType());
                await top.OnCloseAsync();
            }
        }

        public bool IsOpen<TScreen>() where TScreen : class, IScreen
        {
            return _openByType.ContainsKey(typeof(TScreen));
        }
    }
}
