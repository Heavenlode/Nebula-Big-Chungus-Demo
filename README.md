# Big Chungus Online

This is a game made in Godot using the [Nebula](https://github.com/Heavenlode/Nebula) netcode framework. It is the outcome of the [Big Chungus tutorial.](https://nebula.heavenlode.com/tutorials/big-chungus-online/chapter-1-getting-started.html)

![completed-game](https://github.com/user-attachments/assets/a0468a71-3cb8-4269-b2e7-5ac2318c827c)

## How to run

1. Open the project in Godot
    1. You will likely see errors. This is because it hasn't been compiled yet.
3. Build the project
4. Enable the "Nebula" plugin (under project settings)
5. "Customize Run Instances" enable three instances
6. For the first instance:
    1. Check "Override Main Run Args"
    2. Set "Launch Arguments" to `--headless --server`
7. Run the game