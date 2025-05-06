# Game Improvement Plan 2: Daily, Weakly and Seasonal Engagement Systems

This plan focuses on implementing a comprehensive daily engagement system to keep players returning to the game regularly, creating habit-forming gameplay loops similar to other successful games.

## Overview
Transform the piece sorting game into a daily habit by implementing various daily engagement mechanics that offer players compelling reasons to return each day, while maintaining the core gameplay experience.

## Implementation Steps

1. **Daily Mission System**
   - Create 2 daily missions that rotate each day:
     - Complete X pieces of a specific color
     - Create X consecutive piece matches
     - Fill specific positions on the board
     - Use a particular strategy to solve puzzles
   - Award special rewards for completing all daily challenges
   - Implement a streak bonus system for consecutive days of challenge completion
   - Allow the player to hold 5 uncompleted daily challenges until the system starts recycling them

2. **Time-Limited Mini-Games**
   - Create 5-7 rotating of mini-games that appear every 3 days
   - Each mini-game has specific rules and rewards
   - Create leaderboards for players to compete for top scores on each mini-game
   - Each mini-game lasts 72h to encourage quick, while providing entertainment for casual play sessions
   - Mini-Games include:
     - "Sort Points": Currency won for each or specific color match pieces
     - "Color Focus": Only match pieces of a specific color
     - "Color Rush": Board is flooded with specific colored pieces
     - "Pattern Play": Create specific patterns on the board
     - "Team Color Collection": Compete with other teams in a global race to collect the most amount of specific colored pieces
     - "Milestone Rush": Team aims to complete set number of pieces before time runs out
     - "Pattern Masters": Team works to create specific board patterns
     - "Treasure Hunt": Discover hidden pieces on the board

3. **Lives System**
   - Implement a lives-based system that limits gameplay attempts
   - Start with 10 maximum lives with complete recharge every 8 hours
   - Display a countdown timer showing when all lives will regenerate
   - Allow players to watch video ads to restore 2 lives
   - Add friend gifting feature where players can send 1 life to friends daily
   - Create app notifications when lives are fully recharged
   - Design visual indicators for remaining lives with animations for life gain/
   
4. **Piece Upgrade System**
   - Implement a piece upgrade system that allows players to upgrade their pieces
   - Upgraded pieces give more points for matches, allowing levels to be completed faster
   - Upgraded pieces have unique VFX and animations
   - Pieces can be upgraded by consuming specific piece shards
   - Piece shards can be obtained from level completion rewards, daily missions, seasonal pass or mini-games rewards

5. **Seasonal Pass System**
   - Design a series of month seasons with unique themes (Spring Bloom, Summer Heat, etc.)
   - Implement both free and premium reward tracks
   - Premium track rewards include:
     - Exclusive piece VFX
     - Boosters and power-ups
     - Special board backgrounds
     - Piece shards for piece upgrades
     - Unique avatar frames and profile customizations

6. **Community Garden Event**
   - Create regional shared gardens where up to 5,000 players can contribute simultaneously
   - Implement an evolving garden mural with 1,000 x 1,000 tiles spaces that players can contribute to (similar to Reddit's r/place)
   - Design persistent garden states that evolve over time even when individual players are offline
   - Add garden health contribution metrics that reflect collective player contributions
   - Design it as tiled-based collaborative garden canvas system where each completed piece allows players to place colored "flora tiles" on a shared mural
   - Create weekly community objectives where players must collectively arrange certain flower patterns
   - Add a time restriction mechanic where players must wait 5 minutes between "flora tiles" placements to encourage strategic coordination
   - Implement a time-lapse feature showing the garden's evolution over days/weeks
   - Add a player reputation system based on valuable contributions to garden canvas
