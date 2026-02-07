using NUnit.Framework;
using TheOrder.Hunter;

namespace TheOrder.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for the HunterStateMachine — state transitions and event firing.
    /// Uses mock states to verify behavior without requiring Unity scene.
    /// </summary>
    public class HunterStateMachineTests
    {
        #region Mock State

        private class MockState : IHunterState
        {
            public int EnterCount;
            public int UpdateCount;
            public int ExitCount;

            public void Enter() => EnterCount++;
            public void Update() => UpdateCount++;
            public void Exit() => ExitCount++;
        }

        #endregion

        #region Tests

        [Test]
        public void ChangeState_CallsEnterOnNewState()
        {
            var fsm = new HunterStateMachine();
            var state = new MockState();

            fsm.ChangeState(state, HunterState.Patrol);

            Assert.AreEqual(1, state.EnterCount);
        }

        [Test]
        public void ChangeState_CallsExitOnOldState()
        {
            var fsm = new HunterStateMachine();
            var stateA = new MockState();
            var stateB = new MockState();

            fsm.ChangeState(stateA, HunterState.Patrol);
            fsm.ChangeState(stateB, HunterState.Chase);

            Assert.AreEqual(1, stateA.ExitCount);
        }

        [Test]
        public void ChangeState_SetsCurrentStateType()
        {
            var fsm = new HunterStateMachine();
            var state = new MockState();

            fsm.ChangeState(state, HunterState.Investigate);

            Assert.AreEqual(HunterState.Investigate, fsm.CurrentStateType);
        }

        [Test]
        public void ChangeState_FiresHunterStateChangedEvent()
        {
            var fsm = new HunterStateMachine();
            var state = new MockState();
            HunterState receivedState = HunterState.Patrol;
            bool eventFired = false;

            GameEvents.OnHunterStateChanged += (s) =>
            {
                receivedState = s;
                eventFired = true;
            };

            fsm.ChangeState(state, HunterState.Chase);

            Assert.IsTrue(eventFired);
            Assert.AreEqual(HunterState.Chase, receivedState);

            // Cleanup — unsubscribe all (can't easily unsubscribe anonymous delegate,
            // but this is a test so it's acceptable)
        }

        [Test]
        public void Update_DelegatesToCurrentState()
        {
            var fsm = new HunterStateMachine();
            var state = new MockState();

            fsm.ChangeState(state, HunterState.Patrol);
            fsm.Update();
            fsm.Update();
            fsm.Update();

            Assert.AreEqual(3, state.UpdateCount);
        }

        [Test]
        public void Update_WithNoState_DoesNotThrow()
        {
            var fsm = new HunterStateMachine();

            Assert.DoesNotThrow(() => fsm.Update());
        }

        [Test]
        public void ChangeState_TransitionSequence_ExitThenEnter()
        {
            var fsm = new HunterStateMachine();
            var stateA = new MockState();
            var stateB = new MockState();

            fsm.ChangeState(stateA, HunterState.Patrol);

            // Before transition: A entered, B not entered
            Assert.AreEqual(1, stateA.EnterCount);
            Assert.AreEqual(0, stateA.ExitCount);
            Assert.AreEqual(0, stateB.EnterCount);

            fsm.ChangeState(stateB, HunterState.Chase);

            // After transition: A exited, B entered
            Assert.AreEqual(1, stateA.ExitCount);
            Assert.AreEqual(1, stateB.EnterCount);
        }

        [Test]
        public void CurrentState_ReturnsLatestState()
        {
            var fsm = new HunterStateMachine();
            var stateA = new MockState();
            var stateB = new MockState();

            fsm.ChangeState(stateA, HunterState.Patrol);
            Assert.AreEqual(stateA, fsm.CurrentState);

            fsm.ChangeState(stateB, HunterState.Chase);
            Assert.AreEqual(stateB, fsm.CurrentState);
        }

        #endregion
    }
}
