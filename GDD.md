# Power Surge - Game Design Document
This is an updated and simplified version of the original GDD. This version should be referred to when working on the game

## Section 1 - Plot
Set in a world where electricity or power is a mysterious substance produced by 
an ancient energy network buried deep underground near the Earth's core, known as _The World Conductor_.

Recently  _The World Conductor_ has become what people are calling "corrupted". There are mass power outages across the globe, entire cities have been demolished.
Many are fearing this will be the end of humanity.

The protagonist ,[name], is a researcher part of a team that specialises in understanding _The World Conductor_. The team has developed S.U.R.G.E,
a ball of energy that can resist _The World Conductor_'s control. The player must take control of S.U.R.G.E in order to reach the core and shut it down.


## Section 2 - Levels
Levels are split up into 4 different 'sections', there are 2-3 levels per section.

### _The Luminescent Plains_
Set on the surface of the world in large open fields. Here, the player tries to find the entrance to the underground system that will
lead them to _The World Conductor_. Small enemies named circuit bugs patrol the area, guarding the entrance. Other hazards to look out for are energy puddles, and the various puzzles that must be solved along the way.

### _The Circuit Labs_
Once through the underground tunnel found at the end of The _Luminescent Plains_, the player comes accross an abandoned lab, once dedicated to researching _The World Conductor_. Here, the lab's security system has been taken over, creating some new dangerous enemies for the player. The final level is a boss fight against ERR0R, a mutant mech-like robot composed of computers and lab equipment.

To be updated with other 2 sections...

## Section 3 - General Gameplay
The game is a classic 16-bit 2D platformer. 
### Cutscenes
There are non-animated cutscenes at the beginning, end, and between sections. These will have dialogue that is only text-based with no sound, and some pixel art.
### Player Communication
Throughout the levels, dialogue boxes will pop up where the protagonist and other members of the team will comment on what is happening, helping guide the player and build some lore.

## Section 4 - Player Mechanics
### Basic Controls
- Move left/right = a/d or leftarrow/rightarrow
- Jump = w or up arrow. Double Jump = w or up arrow while in mid-air
- Attack = spacebar
- Choose attack = +/-
- Dash = lshift

### Power
The player has one stat - power, it ranges from 0% to 100%.

Power is lost with every move (except just moving left or right), and is also lost when hit by enemy attacks.

Power can be regained by using rechargers or power packs placed throughout the level

If the power level reaches 0%, the player dies and must restart the level

### Power Surge Mode
The player enters Power Surge mode when the power level exceeds 100%.
Power Surge mode increases the speed and jump height of the player as well as access to a new attack.
When the player enters Power Surge mode a timer starts, and if it finishes before the power level returns to 100% or less, the player dies.


### Attacks
| Name              | Description                                                                                   | Power Usage | Damage
|-------------------|-----------------------------------------------------------------------------------------------|-------------|--------
| Weak Pulse        | A short, weak attack used for activating generators. Does a small amount of damage to enemies | 10%         | 10
| Strong Blast      | A powerful, ranged attack, useful for taking out small enemies in one hit.                    | 20%         | 20  
| Power Surge Blast | A very powerful, ranged attack for doing large amounts of damage to multiple enemies          | Half        | 40 
| Surround Blast    | A moderately powerful attack that damages enemies surrounding the player                      | 20%         | 15

### Other Abilities
| Name              | Power Usage
|-------------------|--------------
| Jump              | 3%
| Dash              | 5%
| Shield            | 3%/s

### Death
The player can die in 2 different ways:
- Falling off the edge of the level
- Running out of power


## Section 4 - World Objects
There are lots of different objects to interact with throughout the levels;
| Name               | Function                                                |
|--------------------|---------------------------------------------------------|
| Energy Deposit     | +10% power when walked over                             |
| Battery Pack       | +20% power when walked over                             |
| Electrical Surge   | +40% power when walked over                             |
| Recharger          | +5% power when stood by                                 |
| Engine Fragment    | Collectible Item                                        |
| Generator          | Linked to another object which it powers when activated |
| Moving Platform    | Platform that moves, powered by generator               |
| Retractable Bridge | Bridge that retracts when not powered by generator      |
| Stone Bridge       | Collapses when walked over                              |
| Piston             | Crushes the player, reduces power by 30%                |
| Spring             | Propels the player high into the air when walked over   |
| Energy Shield      | Blocks the Player's path until deactivated              |
| Magnetic Wall      | Allows the player to climb it                           |

## Section 5 - Enemies
There are various enemies scattered across the world:
| Name             | Function                                                      | Description                                                     | Health  |
|------------------|---------------------------------------------------------------|-----------------------------------------------------------------|---------|
| Circuit Bug      | Projectile attack. -10% power                                 | Small, fast moving robotic insect                               | 10      |
| Voltage Sentinel | Short ranged smash attack. -15% power                         | Humanoid robotic guardian designed to protect the circuit labs  | 30      |
| Energy Puddle    | Doesn't move. -5% power when stepped on                       | Small puddles of unstable electricity that appear on the ground | -       |
| Laser Turret     | Projectile attack. -15% power                                 | Turrets leftover from the lab's security system                 | 20      |
| Wild Spark       | Multi-directional shock. -20% power                           | A mysterious electrical entity                                  | 20      |
| Shocking Cloud   | On contact, -35% power                                        | An energy storm that floats                                     | -       |
| Enforcer         | Projectile attack. -20% power. Close-range smash. -30% power. | A large, quadruped robot                                        | 40      |
