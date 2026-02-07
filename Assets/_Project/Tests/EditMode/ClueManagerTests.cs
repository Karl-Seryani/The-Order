using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using TheOrder;
using TheOrder.Clues;

namespace TheOrder.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for ClueManager knowledge level calculation and clue tracking.
    /// </summary>
    [TestFixture]
    public class ClueManagerTests
    {
        #region Helpers

        private ClueManager _manager;
        private GameObject _managerGo;

        [SetUp]
        public void SetUp()
        {
            // Clear singleton from any previous test
            if (ClueManager.Instance != null)
            {
                Object.DestroyImmediate(ClueManager.Instance.gameObject);
            }

            _managerGo = new GameObject("ClueManagerTest");
            _manager = _managerGo.AddComponent<ClueManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_managerGo != null)
            {
                Object.DestroyImmediate(_managerGo);
            }
        }

        private ClueData CreateTestClue(string id, ClueCategory category)
        {
            var clue = ScriptableObject.CreateInstance<ClueData>();
            var so = new SerializedObject(clue);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_category").enumValueIndex = (int)category;
            so.FindProperty("_title").stringValue = $"Test Clue {id}";
            so.ApplyModifiedPropertiesWithoutUndo();
            return clue;
        }

        private void CollectClue(ClueData clue)
        {
            _manager.HandleClueCollected(clue);
        }

        #endregion

        #region Knowledge Level Tests

        [Test]
        public void GetKnowledgeLevel_NoCluesCollected_ReturnsNone()
        {
            Assert.AreEqual(KnowledgeLevel.None, _manager.GetKnowledgeLevel(ClueCategory.Truth));
        }

        [Test]
        public void GetKnowledgeLevel_PartialClues_ReturnsLow()
        {
            CollectClue(CreateTestClue("t1", ClueCategory.Truth));

            Assert.AreEqual(KnowledgeLevel.Low, _manager.GetKnowledgeLevel(ClueCategory.Truth));
        }

        [Test]
        public void GetKnowledgeLevel_HalfClues_ReturnsMedium()
        {
            // 11 truth clues total, half = 5.5, so 6 should be Medium
            for (int i = 0; i < 6; i++)
                CollectClue(CreateTestClue($"t{i}", ClueCategory.Truth));

            Assert.AreEqual(KnowledgeLevel.Medium, _manager.GetKnowledgeLevel(ClueCategory.Truth));
        }

        [Test]
        public void GetKnowledgeLevel_AllClues_ReturnsHigh()
        {
            // 11 truth clues total
            for (int i = 0; i < 11; i++)
                CollectClue(CreateTestClue($"t{i}", ClueCategory.Truth));

            Assert.AreEqual(KnowledgeLevel.High, _manager.GetKnowledgeLevel(ClueCategory.Truth));
        }

        #endregion

        #region Collection Tests

        [Test]
        public void CollectedCount_AfterCollecting_ReturnsCorrectCount()
        {
            CollectClue(CreateTestClue("t1", ClueCategory.Truth));
            CollectClue(CreateTestClue("m1", ClueCategory.Mike));
            CollectClue(CreateTestClue("t2", ClueCategory.Truth));

            Assert.AreEqual(3, _manager.CollectedCount);
        }

        [Test]
        public void IsClueCollected_UnknownClue_ReturnsFalse()
        {
            Assert.IsFalse(_manager.IsClueCollected("nonexistent_id"));
        }

        [Test]
        public void IsClueCollected_AfterCollecting_ReturnsTrue()
        {
            CollectClue(CreateTestClue("t1", ClueCategory.Truth));

            Assert.IsTrue(_manager.IsClueCollected("t1"));
        }

        [Test]
        public void CollectDuplicate_DoesNotDoubleCount()
        {
            var clue = CreateTestClue("t1", ClueCategory.Truth);
            CollectClue(clue);
            CollectClue(clue);

            Assert.AreEqual(1, _manager.CollectedCount);
        }

        [Test]
        public void GetCategoryCount_MixedCategories_ReturnsPerCategory()
        {
            CollectClue(CreateTestClue("t1", ClueCategory.Truth));
            CollectClue(CreateTestClue("t2", ClueCategory.Truth));
            CollectClue(CreateTestClue("m1", ClueCategory.Mike));

            Assert.AreEqual(2, _manager.GetCategoryCount(ClueCategory.Truth));
            Assert.AreEqual(1, _manager.GetCategoryCount(ClueCategory.Mike));
        }

        [Test]
        public void GetCollectedCluesByCategory_ReturnsOnlyMatchingCategory()
        {
            CollectClue(CreateTestClue("t1", ClueCategory.Truth));
            CollectClue(CreateTestClue("m1", ClueCategory.Mike));
            CollectClue(CreateTestClue("t2", ClueCategory.Truth));

            var truthClues = _manager.GetCollectedCluesByCategory(ClueCategory.Truth);

            Assert.AreEqual(2, truthClues.Count);
            foreach (var clue in truthClues)
                Assert.AreEqual(ClueCategory.Truth, clue.Category);
        }

        #endregion
    }
}
