# Evolute Kingdom: Mage Duel - Unity Client

Evolute Kingdom: Mage Duel is an on-chain game, built on Starknet using the [Dojo Engine](https://github.com/dojoengine/dojo)

Information about on-chain part can be found here: [territory-wars-dojo](https://github.com/evolute-studio/territory-wars-dojo)

Playbook (Lore and Game Rules): https://evolute.notion.site/playbook

Play Evolute Kingdom - Mage Duel 👉 https://mageduel.evolute.network/

# **Development Setup**

- [Unity Editor](https://unity.com/releases/editor/whats-new/6000.0.25#notes): Unity 6 6000.0.25f1
- [Dojo Unity SDK](https://github.com/dojoengine/dojo.unity): v1.2.1
- [Sozo](https://book.dojoengine.org/toolchain/sozo), [Katana](https://book.dojoengine.org/toolchain/katana), [Torii](https://book.dojoengine.org/toolchain/torii): v1.2.1

# **Running the Project**

To run the client, follow simple steps:

1. Open the scene `Assets/Scenes/Start.scene`
2. Find the game object: `WorldManager`.
3. In the `WorldManager` and `DojoGameManager` components, set the connection configuration in the `Dojo Config` field. An example of configurations can be found here: `Assets/TerritoryWars/DojoConfig/`.
4. Set the address for the `Game` and `Player_profile_actions` contracts

# **Building the Project**
To build, you need to select the target platform **WebGL**:

1. Select the platform as a target in `Build Profiles`
2. Select the `Dojo` template in `PlayerSettings` (a fix has been added to the template to solve the problem with the inability to create a Burner Account after page overload)
3. Build the project
4. In the folder with the finished build, there will be a simple python web server `unity_server.py`
