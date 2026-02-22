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
                "  ESC  -  Pause"
            },
            // The Hunter
            new[]
            {
                "A monster stalks these halls.\n\n" +
                "He growls. He never rests.\n" +
                "He patrols the corridors searching\n" +
                "for you.\n\n" +
                "If he sees you, he will chase.\n" +
                "If he catches you, it is over.\n\n" +
                "On EASY, the Hunter detects you by\n" +
                "sight only. No flashlight, sprint, or\n" +
                "noise detection.\n\n" +
                "On MEDIUM and HARD, he hears your\n" +
                "footsteps, doors, and flashlight.\n" +
                "Every sound you make is a risk."
            },
            // Survival
            new[]
            {
                "3 DAYS TO ESCAPE\n\n" +
                "You have 3 days. Each death costs one day.\n" +
                "Your progress carries over between deaths.\n" +
                "Unlocked doors, used items, and keys persist.\n" +
                "On Day 3, it is your last chance.\n\n" +
                "LIGHT AND DARKNESS\n\n" +
                "Your flashlight reveals the path ahead\n" +
                "but also makes you visible from afar.\n\n" +
                "SOUND\n\n" +
                "Sprinting echoes through the halls.\n" +
                "Walk slowly to stay silent.\n" +
                "Opening doors and drawers creates noise.\n\n",
                // Page 2
                "ITEM CHAIN\n\n" +
                "Hammer - breaks barricades\n" +
                "Screwdriver - unscrews locked drawers\n" +
                "Knife - cuts open cushions\n" +
                "Wrench - pries open the morgue door\n\n" +
                "Search shelves, drawers, and mugs\n" +
                "to find what you need.\n\n" +
                "CLUES\n\n" +
                "2 documents are hidden in the bunker.\n" +
                "Press E to read, then E again to collect.\n" +
                "They hold useful information."
            },
            // Escape
            new[]
            {
                "PRACTICE\n\n" +
                "No Hunter. Get familiar with the layout\n" +
                "and item locations. Build the car to escape.\n\n" +
                "EASY\n\n" +
                "Hunter patrols but only detects by sight.\n" +
                "Find the Main Door Key to escape.\n\n" +
                "MEDIUM\n\n" +
                "Hunter hears everything. Stay quiet.\n" +
                "Find the Main Door Key to escape.\n\n" +
                "HARD\n\n" +
                "Same detection as Medium, but you must\n" +
                "also build the car. Find 3 wheels + motor,\n" +
                "place them on the car frame outside.\n" +
                "Drill the wheels in. Find the car key\n" +
                "in the 1st floor kitchen. Start the engine."
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
            {
                _sectionTitle.text = SECTION_TITLES[_currentSection];
            }

            if (_bodyText != null)
            {
                _bodyText.text = SECTION_PAGES[_currentSection][_currentPage];
            }

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
