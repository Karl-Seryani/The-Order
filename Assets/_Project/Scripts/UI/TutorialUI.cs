using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Multi-page tutorial with tabbed sections: Controls, Survival Tips, Clues &amp; Endings.
    /// Navigated via tab buttons, prev/next, and back to main menu.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private MainMenuUI _mainMenuUI;
        [SerializeField] private Text _sectionTitle;
        [SerializeField] private Text _bodyText;
        [SerializeField] private Text _pageIndicator;

        [Header("Navigation")]
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _backButton;

        [Header("Tabs")]
        [SerializeField] private Button _controlsTab;
        [SerializeField] private Button _survivalTab;
        [SerializeField] private Button _cluesTab;

        #endregion

        #region Private Fields

        private static readonly string[] SECTION_TITLES =
        {
            "Controls",
            "Survival Tips",
            "Clues & Escape"
        };

        private static readonly string[][] SECTION_PAGES =
        {
            // Controls
            new[]
            {
                "MOVEMENT\n" +
                "W A S D      Walk\n" +
                "Shift          Sprint\n" +
                "Mouse         Look around\n\n" +
                "ACTIONS\n" +
                "E                Interact\n" +
                "F                Flashlight\n" +
                "Q               Drop item\n\n" +
                "INTERFACE\n" +
                "Tab             Objective\n" +
                "Esc             Pause"
            },
            // Survival Tips
            new[]
            {
                "Hide in lockers to avoid the Hunter. Otherwise,\n" +
                "outrun him and break line of sight around corners.\n\n" +
                "Your flashlight reveals the world but also attracts\n" +
                "attention. In darkness, the Hunter can barely see.\n" +
                "use that to your advantage.\n\n" +
                "Sprinting drains your stamina. If you run dry, you\n" +
                "will be forced to walk until it recovers.\n\n" +
                "Doors can be opened and closed. A closed door\n" +
                "buys you time. The Hunter must open it to follow.\n\n" +
                "Stay quiet. Walking slowly makes almost no noise,\n" +
                "but sprinting can be heard from a distance."
            },
            // Clues & Endings
            new[]
            {
                "Scattered throughout the bunker are 18 clue documents\n" +
                "that reveal the truth about this place.\n\n" +
                "  COLLECTING CLUES\n" +
                "  Approach a document and press E to read it.\n" +
                "  Press E again to collect it.\n\n" +
                "  YOUR GOAL\n" +
                "  Find keys, unlock new areas, and locate the exit.\n" +
                "  The more clues you find, the more you understand\n" +
                "  what happened here and what the Hunter really is.\n\n" +
                "  Explore carefully. Every room could hold a vital clue\n" +
                "  or the Hunter himself."
            }
        };

        private int _currentSection;
        private int _currentPage;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_prevButton != null) _prevButton.onClick.AddListener(OnPrevClicked);
            if (_nextButton != null) _nextButton.onClick.AddListener(OnNextClicked);
            if (_backButton != null) _backButton.onClick.AddListener(OnBackClicked);
            if (_controlsTab != null) _controlsTab.onClick.AddListener(() => GoToSection(0));
            if (_survivalTab != null) _survivalTab.onClick.AddListener(() => GoToSection(1));
            if (_cluesTab != null) _cluesTab.onClick.AddListener(() => GoToSection(2));
        }

        private void OnEnable()
        {
            // Reset to first section when tutorial opens
            _currentSection = 0;
            _currentPage = 0;
            UpdateDisplay();
        }

        #endregion

        #region Navigation

        private void GoToSection(int section)
        {
            if (section < 0 || section >= SECTION_PAGES.Length) return;

            _currentSection = section;
            _currentPage = 0;
            UpdateDisplay();
        }

        private void OnPrevClicked()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
            }
            else if (_currentSection > 0)
            {
                _currentSection--;
                _currentPage = SECTION_PAGES[_currentSection].Length - 1;
            }
            UpdateDisplay();
        }

        private void OnNextClicked()
        {
            if (_currentPage < SECTION_PAGES[_currentSection].Length - 1)
            {
                _currentPage++;
            }
            else if (_currentSection < SECTION_PAGES.Length - 1)
            {
                _currentSection++;
                _currentPage = 0;
            }
            UpdateDisplay();
        }

        private void OnBackClicked()
        {
            if (_mainMenuUI != null)
                _mainMenuUI.ShowMainMenu();
        }

        #endregion

        #region Display

        private void UpdateDisplay()
        {
            if (_sectionTitle != null)
                _sectionTitle.text = SECTION_TITLES[_currentSection];

            if (_bodyText != null)
                _bodyText.text = SECTION_PAGES[_currentSection][_currentPage];

            int totalPages = SECTION_PAGES[_currentSection].Length;
            if (_pageIndicator != null)
                _pageIndicator.text = $"{_currentPage + 1}/{totalPages}";

            // Update nav button interactability
            bool canGoPrev = _currentSection > 0 || _currentPage > 0;
            bool canGoNext = _currentSection < SECTION_PAGES.Length - 1
                          || _currentPage < SECTION_PAGES[_currentSection].Length - 1;

            if (_prevButton != null) _prevButton.interactable = canGoPrev;
            if (_nextButton != null) _nextButton.interactable = canGoNext;
        }

        #endregion
    }
}
