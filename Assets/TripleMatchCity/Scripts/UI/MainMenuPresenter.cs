using System;
using TripleMatch.Core;
using TripleMatch.Level;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace TripleMatch.UI
{
    /// <summary>
    /// Wires the main menu view to the rest of the game.
    ///   - Title text is set once.
    ///   - Next level text reflects LevelManager.CurrentDisplayIndex.
    ///   - Play raises GameSceneRequestedEvent (SceneFlowService loads the Game scene).
    ///   - Clear resets level progression and refreshes the next level text.
    /// </summary>
    public class MainMenuPresenter : IStartable, IDisposable
    {
        private const string TitleString = "Clone City";
        private const string NextLevelFormat = "{0}";

        private readonly MainMenuView _view;
        private readonly ILevelManager _levelManager;
        private readonly IEventBus _eventBus;

        public MainMenuPresenter(MainMenuView view, ILevelManager levelManager, IEventBus eventBus)
        {
            _view = view;
            _levelManager = levelManager;
            _eventBus = eventBus;
        }

        public void Start()
        {
            if (_view == null) return;

            SetTitle();
            RefreshNextLevel();

            Button play = _view.PlayButton;
            if (play != null) play.onClick.AddListener(OnPlayClicked);

            Button clear = _view.ClearButton;
            if (clear != null) clear.onClick.AddListener(OnClearClicked);
        }

        public void Dispose()
        {
            if (_view == null) return;

            Button play = _view.PlayButton;
            if (play != null) play.onClick.RemoveListener(OnPlayClicked);

            Button clear = _view.ClearButton;
            if (clear != null) clear.onClick.RemoveListener(OnClearClicked);
        }

        private void SetTitle()
        {
            if (_view.TitleText != null) _view.TitleText.text = TitleString;
        }

        private void RefreshNextLevel()
        {
            if (_view.NextLevelText == null || _levelManager == null) return;
            _view.NextLevelText.text = string.Format(NextLevelFormat, _levelManager.CurrentDisplayIndex);
        }

        private void OnPlayClicked()
        {
            _eventBus.Raise(new GameSceneRequestedEvent());
        }

        private void OnClearClicked()
        {
            _levelManager?.ResetProgress();
            RefreshNextLevel();
        }
    }
}
