using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TheOrder.Tests.PlayMode
{
    /// <summary>
    /// PlayMode smoke test to verify the PlayMode test runner is working.
    /// </summary>
    public class PlayModeSmokeTest
    {
        [UnityTest]
        public IEnumerator TestRunner_Works()
        {
            yield return null;
            Assert.Pass("PlayMode test runner is operational.");
        }
    }
}
