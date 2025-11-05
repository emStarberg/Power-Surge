# Milestone 4 - braithemil
## Good Code
**#1 - Dialogue**

The dialogue system is more complex that it appears. 

DialogueBox.cs is full of functions to make adding dialogue to a level seamless:

https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Scripts/UI/DialogueBox.cs

it parses a .txt file such as this one I used for the opening sequence and automatically loads it into a set of DialogueLines:

https://github.com/emStarberg/Power-Surge/blob/main/Power%20Surge/Assets/Dialogue%20Files/openingsequence.txt

DialogueLine is its own class within DialogueBox.cs, it stores the text. as well as the name of the speaker and finds the correct image to display based on the speaker name.


I believe this code is good because it creates a more efficient system overall, it made integrating dialogue with each level so much easier. It’s also one of the classes with the most descriptive and helpful comments.



**#2 - **
