# Game Improvement Plan 1: Progressive Difficulty System

This plan focuses on adding a progressive difficulty system and game design mechanics to make the game more engaging and challenging over time.

## Overview

- Implement a level-based progression system where the game becomes increasingly difficult as the player advances, adding a sense of achievement to keep the player engaged longer.
- Create 5 initial difficulty levels that are easy to start with, but become increasingly challenging as the player progresses.
- Each level increases the complexity of pieces and tile grid limitations to maintain engagement.
- Offer strategic "breakthrough moments" for each level for player satisfaction with power-ups, rewards, special tiles and pieces.

## Implementation Steps

1. **Improve the Game Logic**
   - Process the level's progression and difficulty settings in the GameLevelLogic.cs
   - Add level-specific configurations containing the difficulty settings, completion criteria and rewards for each level
   - Create a reward system to reward the player on completing levels based on each level-specific configuration
   - Create a point system that is awarded based on the completion of pieces on the board
   - Add levels objectives that start with simple tasks (e.g. complete 3 red pieces) and evolve to complex ones (e.g. complete/clear 3 special tiles) as the game progresses
   - Implement a star rating system for level completion based on the points earned
   - Implement a bonus system that awards x2 points for 2 consecutive matches, x3 for 3 matches, and so on
   - Add analytics tracking to measure player engagement metrics (completion time, moves used, etc.)

2. **Level Design Difficulty**
   - Each level introduces new mechanics:
     - Level 1: Basic piece matching
     - Level 2: Color mixing challenges
     - Level 3: Time-based objectives
     - Level 4: Multi-piece combinations
     - Level 5: Special tile types
   - Introduce tiles and pieces that increase difficulty:
     - Level 1: 4 colors (White, Black, Red, Yellow) for beginners
     - Level 2: 6 colors (all available colors)
     - Level 3: 6 colors with predetermined difficult piece sequences that follow an escalating pattern for matching colors
     - Level 4: 6 colors with new multi-piece type shapes spawned on deck
     - Level 5: 6 colors and special tile types
   - Implement progressive challenge mechanics:
     - Level 1: Basic matching until goal completed without any restrictions
     - Level 2: Introduce "target pieces" - specific colored pieces that must be completed to win the level
     - Level 3: Implement "time challenges" where players must be quick to achieve specific color combinations before time expires
     - Level 4: Introduce "special objectives" that require the cleanup of pieces from the grid board before the objective item can be revealed. When fully revealed, the objective item flies to the UI objective indicator to indicate the player's progress towards the level completion
     - Level 5: Introduce "Frozen Tiles" a combination of special tiles type that requires a specific gameplay puzzle of pieces for the player to complete

3. **Piece Shape Types**
   - 1x1 single pieces
   - 2x1 horizontal pieces
   - 1x2 vertical pieces
   - 2x2 square pieces
   - L-shape pieces
   - Z-shape pieces
   - T-shape pieces

4. **Tile Types for Level 5**
   - "Frozen Tiles": Tiles that are locked in ice and need to be broken before pieces can be sorted with it. To break an ice tile, a piece needs to be completed next to it.
   - "Tile Blockers": Tiles that prevent matching of specific colors or pieces in specific positions or direction
   - "Slice Transfer Limiters": Tiles that restrict the number of slice transfers per turn
   - "Timed Tiles": Tiles where a specific Slice Color/s must be matched within a certain number of moves or spawns new Pieces in its place and adjacent tiles
   - "Color Waves": Tiles that will swap Slice Colors within a certain number of moves
   - "Growing Slices": Tiles that will spawn new Slice Colors in the piece containing it within a certain number of moves
   - "Moving Tiles": Tile lines that move to specific directions after each turn

5. **Power-Up System**
   - Add special power seeds that appear after 2 or more combo matches. Each seed contains a power-up when activated by the player clicking it or dropping a piece on the tile containing it
   - Add new power ups for the player to use strategically during gameplay
     - Color Swap: Change a piece's color to another (limited uses)
     - Clear Tile: Remove any piece from the board
     - Wild Piece: Complete piece that all Slices will adjust it's color when dropped to optimal value to clean as many tiles as possible
     - Auto-Sort: Automatically optimize a specific piece and its surroundings
     - Piece Preview: Pick a piece, Archero rogue-like style to drag and drop it on the board

6. **UI and VFX Feedback**
   - Add particle effects with VFX when a level is completed
   - Add a visual level indicator on the game screen
   - Add level objective progress completion into the UI
   - Design power-up icons and visual effects when each power up is used
   - Add point animation for each piece completed to indicate the player's progress towards the level completion
   - Add a reward UI panel to display the end of the level rewards won
   - Create a power-up selector interface at the side of the board
   - Add UI tooltips for each power-up icon to explain its functionality
