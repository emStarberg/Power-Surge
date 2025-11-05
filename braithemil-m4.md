# Milestone 4 - braithemil
## Good Code
**#1 - Dialogue**

The dialogue system is more complex that it appears. 

[DialogueBox.cs](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/UI/DialogueBox.cs) is full of functions to make adding dialogue to a level seamless:, it parses a .txt file such as [this](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Assets/Dialogue%20Files/openingsequence.txt) one I used for the opening sequence and automatically loads it into a set of DialogueLines.


DialogueLine is its own class within DialogueBox.cs, it stores the text. as well as the name of the speaker and finds the correct image to display based on the speaker name.


I believe this code is good because it creates a more efficient system overall, it made integrating dialogue with each level so much easier. It’s also one of the classes with the most descriptive and helpful comments.


**#2 - Title Screen**

[TitleScreen.cs](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/UI/TitleScreen.cs) was my first UI scene, I created a system for navigating through buttons, and adding an effect whilst doing so. This system was reused for multiple other classes/scenes. One thing to note is that my first few UI scenes had Node2D as the root node, I corrected this to use Control in later scenes.

**#3 - Lab Boss**

[LabBoss.cs](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/Enemies/LabBoss.cs) was a huge challenge for me, being the first time I’ve programmed a boss fight. I think that I made a good set of public methods to make it easier for the level node to control. There were so many different moving parts, and different actions for the boss, I think I did well getting it all to work together in a natural way.

**#4 - Enemy**

[Enemy.cs](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/Enemies/Enemy.cs) is an abstract class inherited by enemies such as the [Circuit Bug](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/Enemies/CircuitBug.cs).


I used an abstract class in this way to keep the enemy scripts from becoming too bloated with very similar code. Some of the methods are virtual, so that special cases such as the lab boss were able to override them. Methods such as Hurt() were always going to do the same thing for each enemy, remove some amount of health and flash red for a second. 

I was initially planning to have a larger cast of enemies, there would’ve been a lot more code to copy over without this abstract class. It was so effective that I was able to use it for the big hammers that attempt to crush the player during the final boss, despite them not being typical enemies.

**#5 - Switch Operated Objects**

I needed a way to easily connect switches to gates and elevators when designing the puzzle level, which is where [SwitchOperatedObject.cs](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/Objects/SwitchOperatedObject.cs) comes in.


Any object that can be turned on/off by a [Switch](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/Objects/Switch.cs) inherits SwitchOperatedObject. This means that a Switch can just be given a list of SwitchOperatedObjects and be instructed to iterate through and turn them on/off, with no concern for the nature of the actual objects. When I needed to add moving platforms into a later level, it only took me 5 minutes thanks to this already being set up.



## Bad Code

**#1 - Camera**

[Camera.cs](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/Other/Camera.cs) is the script attached to the camera in each level. 

I think that the end result from this script is good, the camera transitions between modes smoothly to give the player a better field of view depending on the environment. Unfortunately the code is not great. I basically just threw another thing in there every time I needed a new camera mode without integrating it properly, modes work very differently from each other and worst of all, calling a mode change from another script is different between modes. If you want to use the horizontal camera mode you simply write:

- camera.Mode = “horizontal”,

but if you want the camera to remain centered on the y axis you write: 

- camera.Mode = “centered”;  

- camera.SetCenterY(-350f); 

- camera.ChangeToCentered(); 

Centered is also a terrible name for that mode because it sounds like it would center both the x and y axis.

**#2 - Player**

[Player.cs](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/Player/Player.cs) is the script used for player movements… and some other stuff that really should’ve been put in a separate script.

Player.cs is a pretty large file at 780 lines. It has methods for every player action: move left/right, jump, dash, attack, switch attack, hurt, die. This is already a lot, but on top of that I had it manage the power meter as well. In an ideal world, the power meter would’ve been handled in its own script, making use of public methods in player to retrieve the required data.

Because it was one of the first scripts created and was worked on in pieces throughout the whole project, it turned into a bit of an inconsistent mess my as code quality and style improved overtime.

**#3 - GameData**

[GameData.cs](https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/Other/GameData.cs) is perfectly functional but a pretty awful bit of code as half of it is useless. I had an old system for making objects glow depending on the level which I scrapped but forgot to remove here.




