using System;
using UnityEngine;
using UnityEngine.InputSystem;
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
        [SerializeField] private Button _hunterTab;
        [SerializeField] private Button _survivalTab;
        [SerializeField] private Button _escapeTab;

        #endregion

        #region Public API

        /// <summary>Override back action for use outside main menu (e.g. pause menu).</summary>
        public Action OnBackAction { get; set; }

        #endregion

        #region Private Fields

        private static readonly string[] SECTION_TITLES =
        {
            "Controls",
            "The Hunter",
            "Survival",
            "Escape"
        };

        private static readonly string[][] SECTION_PAGES =
        {
            // Controls
            new[]
            {
                "MOVEMENT\n\n" +
                "  W A S D  -  Walk\n" +
                "  SHIFT  -  Sprint\n" +
                "  MOUSE  -  Look around\n\n" +
                "ACTIONS\n\n" +
                "  E  -  Interact / Pick up / Read\n" +
                "  F  -  Toggle flashlight\n" +
                "  Q  -  Drop held item\n\n" +
                "INTERFACE\n\n" +
                "  TAB  -  Show current objective\n" +
                "  ESC  -  Pause"
            },
            // The Hunter
            new[]
            {
                "Something stalks these halls.\n\n" +
                "He does not speak. He does not rest.\n" +
                "He patrols the corridors in silence,\n" +
                "searching for you.\n\n" +
                "If he sees you, he will chase.\n" +
                "If he catches you, it is over.\n\n" +
                "On EASY difficulty, the Hunter can only\n" +
                "detect you by sight. Stay out of his\n" +
                "line of sight and you are safe.\n\n" +
                "On MEDIUM and HARD, he also hears\n" +
                "your footsteps, doors, and flashlight.\n" +
                "Every sound you make is a risk."
            },
            // Survival
            new[]
            {
                "LIGHT AND DARKNESS\n\n" +
                "Your flashlight reveals the way forward\n" +
                "but also makes you visible from far away.\n" +
                "In darkness, the Hunter's sight is limited.\n" +
                "Use that to your advantage.\n\n" +
                "SOUND\n\n" +
                "Sprinting echoes through the halls.\n" +
                "Walk slowly to stay silent.\n" +
                "Opening doors and drawers creates noise.\n\n" +
                "STAMINA\n\n" +
                "Sprinting drains your stamina. If it runs\n" +
                "out, you cannot sprint until it recovers.\n" +
                "Manage it wisely during a chase.",
                // Page 2
                "DOORS\n\n" +
                "Doors can be opened and closed freely.\n" +
                "A closed door buys you precious seconds.\n" +
                "The Hunter must stop to open it.\n\n" +
                "KEYS AND LOCKED AREAS\n\n" +
                "Some doors require specific keys.\n" +
                "Search drawers, shelves, and hidden\n" +
                "corners to find them.\n\n" +
                "CLUES\n\n" +
                "18 documents are scattered throughout\n" +
                "the bunker. Approach one and press E\n" +
                "to read it, then E again to collect.\n" +
                "They reveal the truth about this place."
            },
            // Escape
            new[]
            {
                "PRACTICE MODE\n\n" +
                "No Hunter. Explore freely and learn the\n" +
                "layout. Repair the car to escape.\n\n" +
                "EASY MODE\n\n" +
                "The Hunter patrols but can only see you.\n" +
                "Find the main door and press E to escape.\n\n" +
                "MEDIUM MODE\n\n" +
                "The Hunter hears everything. Stay quiet.\n" +
                "Find the main door and press E to escape.\n\n" +
                "HARD MODE\n\n" +
                "Full Hunter. No easy way out.\n" +
                "Find 4 car parts scattered in the bunker.\n" +
                "Carry them outside to the car frame.\n" +
                "Use the drill on the wheels. Find the\n" +
                "car key. Start the engine. Drive away."
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
            if (_hunterTab != null) _hunterTab.onClick.AddListener(() => GoToSection(1));
            if (_survivalTab != null) _survivalTab.onClick.AddListener(() => GoToSection(2));
            if (_escapeTab != null) _escapeTab.onClick.AddListener(() => GoToSection(3));
        }

        private void OnEnable()
        {
            // Reset to first section when tutorial opens
            _currentSection = 0;
            _currentPage = 0;
            UpdateDisplay();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                OnBackClicked();
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
            if (OnBackAction != null)
                OnBackAction();
            else if (_mainMenuUI != null)
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
