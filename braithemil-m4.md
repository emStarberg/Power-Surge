# Milestone 4 - braithemil - 300617442

## Milestone 3 Summary
https://github.com/emStarberg/Power-Surge/blob/main/milestone3.md

## Video
https://youtu.be/vpSl9jbs23U

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


## Personal Reflection

I learned an enormous amount in the duration of this project. Every aspect of the game development was left up to me, from the coding that I’m comfortable with, through asset creation which I have some experience with, all the way to writing and game design which is definitely a weakness of mine.

The scope of my original design assumed that I would be working in a team of two, and although I had to cut a fair amount of planned content,  I believe I got more out of the experience by working solo.

I was fortunate enough to already have my prototype to follow for the most basic game mechanics, but I didn’t copy the code directly, I used it as a guide to write better code with a more robust structure.
I learned so many new concepts I couldn’t talk about them all, but I’ll mention a few notable ones:

**Architecture of a C# project**

At the beginning, my only experience of C# was from using it in a couple of game jams with Godot, it was similar enough to Java that I could pick it up quickly, but I never looked into things like naming conventions or even documentation because I was always just racing to finish a jam.

I decided that if I was going to make a larger game, I would need to carefully plan the structure of my code. There’s a class diagram that I created here. It was done quite a while ago so it’s not up to date, but it gives the general idea. I had to learn how to use abstract classes and interfaces to my advantage which is a great skill to have.

**Time Management**

It’s very hard to quantify how much time things take in a larger project when so much is new to you. I was very relaxed about my time until one day I realised I hadn’t worked on it for 5 days despite not being busy, from then on, I worked on it almost every night for around 4 hours,
setting a goal at the beginning of each night that needed to be completed. 

Asset creation was surprisingly non-time consuming, pixel art on a small scale is quite easy to get done once you figure out what you’re doing, I also have a background in 2D animation which helped a lot.
I got the vast majority of what I planned finished, there were a few objects and enemies that didn’t make the final cut, but the most important thing was getting all the big features in there, and I ended up with a finished game.

**Working in a Team**

This one might seem weird, I had no team, but one of the quickest ways to understand someone is by stepping into their shoes. 

For this project, I was no longer simply a programmer, I had to be an artist, a writer, an animator, a game designer, a project manager. I feel that one of the most valuable things I got out of this course was a deeper understanding of the perspective of others, I’ve now had a taste of the work involved in a variety of roles.

Of course, this doesn’t mean I could ever do any of those roles in a team on a real project, programming is my strength.

The point I’m trying to get across is that even through working solo, I’ve gained some valuable insights into working with others.

**Conclusion**

I will take everything I’ve learned during this project and put it to good use in my future work, I’ve noticed a big difference in my general code quality and ability to problem solve which will always be useful but particularly time management and just the all round game development skills I’ve learned.

Overall, I’m very pleased with how my game turned out. I was always going to wish I’d done more no matter what but I believe I’ve put in my best effort.






