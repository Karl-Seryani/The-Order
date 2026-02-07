Implement a new game system for The Order.

System name: $ARGUMENTS

Steps:
1. Read docs/ARCHITECTURE.md for event bus patterns
2. Read existing Core/GameEvents.cs for current events
3. Create the script in the appropriate Assets/_Project/Scripts/ subfolder
4. Use TheOrder namespace
5. Wire into GameEvents.cs (add new events if needed)
6. Create EditMode tests in Assets/_Project/Tests/EditMode/
7. Verify compilation via Unity console
8. Update docs if architecture changes
