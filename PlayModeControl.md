# Play Modes & Time Control

These tools combine features from PlayStateDevTool, SceneSeqDevTool, and TimeScaleOverlay to provide comprehensive control over game execution and sequence flows directly from your mapped keyboard 
shortcuts.

## Play State Controls
Allows for rapid Editor playback manipulation without relying on Unity's default mouse-driven UI.
- Toggle Play: Enters or exits Play Mode.
- Toggle Pause: Pauses or unpauses the current execution.
- Step: Advances the game by a single frame (automatically forces a pause if currently playing).

## Scene Sequence Tools
Integrates with the SceneSequenceController to load and test specific game scenarios.
- Run Selected Sequence: Instantly boots the sequence currently highlighted in your configuration.
- Run Start Scene As Release: Boots the game from the primary initialization scene to simulate a production build environment.
- Select Next Sequence: Cycles to the next available sequence.

## Time Scale Overlay
An interactive UI overlay rendered directly in the Unity Scene View.
- Real-time Monitoring: Displays the current game speed alongside status icons.
- Status Indicators: Visually updates with clear emoji icons (🕑 for playing, ⏸️ for paused) and changes text color to red when paused.
- Live Feedback: Updates continuously via EditorApplication.update, ensuring accurate timescale values are displayed even when modifying speeds through custom tools while the game is actively paused.
