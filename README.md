# SlayTheRPG

A 2D turn-based RPG built in Unity. The player fights through three encounters, and a final boss with a distinct four-turn cycle, in a short combat loop with its own AI variety, progression, and presentation layer.

*![Gameplay](https://github.com/mosunna/SlaytheRPG/blob/main/Media/videotogif.gif)*

**Play it:** *[https://mosunna.itch.io/slaytherpg]*

## About the Game

SlayTheRPG is a turn-based RPG about reading an enemy's next move and answering it correctly. Each turn, the player picks one of four actions: **Attack** a chosen target, **Defend** to raise defense for the turn, or spend FP on **Heal** or **Charge** (which doubles the player's next attack). Enemies telegraph their intent before acting, through an intent icon and a narrated action log, so the right response is usually readable in advance rather than guessed at.

The goal is to clear all three regular encounters, then survive and defeat the final boss, Lavos, whose fixed four-turn cycle (Charge, Attack, Shield, Expose) turns each of its turns into a specific decision for the player: brace for the incoming hit, or press the advantage while its core is exposed.

## Overview

Title screen leads into name entry, then a level select screen with three regular encounters and a final boss. Clearing all three regular encounters grants a one-time stat boost ahead of the boss fight. Winning or losing a battle returns to level select or offers a restart, and beating the boss routes to a distinct ending screen rather than looping back into the normal level-clear flow.

## Combat systems

- **Turn-based state machine** driving the full battle sequence: enemies spawn, choose their intent, the player acts, enemies act, then win/loss is checked before looping back.
- **Player actions:** targeted Attack, Defend (temporary defense bonus), and two FP-cost spells, Heal and Charge (Charge doubles the player's next attack).
- **Enemy AI variety:**
  - A base enemy type that attacks, with a variable number of hits per turn, and can roll to Defend instead, raising its own defense for the turn (used by the Goblin and the Carnivorous Plant, each tuned with their own defend chance and bonus).
  - A Slime that splits into two smaller slimes when it crosses 50% of its max HP, each inheriting its exact current HP as their own new max HP, so a single slime dropped low enough by one hit turns into a spread of enemies rather than just dying.
  - A Lunatic Cultist that buffs a living ally's attack if one exists, or attacks directly if it's alone.
  - The final boss, Lavos, a fixed four-turn cycle: Charge (sets up its next hit), Attack (lands the charged, doubled hit), Shield (bonus defense), and Expose (a window of 6x damage taken before it recovers).
- **Enemy intent icons** telegraph what an enemy is about to do before it acts.
- **A narrated action log** describes each turn with text specific to what happened, the boss's four turns in particular form a small causal story across a cycle (bracing causes fatigue, fatigue causes the opening) rather than generic flavor text.
- **Damage math:** rolled variance per hit, defense mitigation floored at a 1-damage minimum, and the boss's own damage-multiplier states layered on top.
- Visual combat feedback: a damage flash on hit and a gray tint once a character is defeated, plus animated HP/FP bars with a trailing "ghost" bar that catches up after each hit.

## Progression & state

- Encounters and enemies are fully data-driven through ScriptableObjects, adding a new level or enemy doesn't require touching combat code.
- A persistent manager carries the player's name, selected encounter, and current stats across scene loads, so progress survives between the menu and battle scenes.
- A percentage-based HP/FP recovery runs between encounters, easing the run's attrition without removing it.
- The final boss encounter ends the game through its own distinct ending screen and music cue, not the regular level-clear loop.


## Technical Highlights

**Architecture.** Combat runs as an explicit state machine (`TurnManager`), rather than a sequence of hardcoded coroutines, so the flow between spawning, intent selection, player input, enemy resolution, and win/loss checking stays a single readable loop instead of scattered logic. All combatants share one abstract `Character` base holding damage resolution, defense, buffs, and the HP/FP bar tweening, so `Player`, `Enemy`, `Boss`, and `LunaticCultist` differ only where they actually need to, `Boss` and `LunaticCultist` override just `ChooseNextIntent()` and `ExecuteIntent()` to get entirely different behavior for free from the shared base. Encounters and enemies are ScriptableObject data assets rather than scene objects, so new content is authored as data, not code. A persistent manager (`DontDestroyOnLoad`, guarded against duplicate instances) carries run state, player stats, and now certain audio across scene boundaries.

**Cross-scene communication.** Unity object references don't survive a scene load, so anything that needs to reach the next scene goes through simple persisted state on the manager rather than a direct reference, a boolean flag set before the load tells the Main Menu scene whether to show level select or the boss ending screen, for example, instead of either scene needing to know about the other's objects. The same single-source approach shows up within a scene too: each enemy's intent is decided once per turn and stored as one `Intent` value, then two independent systems, the action log and the intent icon, both read from that same value rather than each re-deciding or duplicating what the enemy is about to do.



## References

*Battle transition: https://github.com/ickybodclay/UnityRpgOverworldBattleExample*
