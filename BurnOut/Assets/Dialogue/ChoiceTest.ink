->main

=== main ===
This is test dialogue. #name: Mentor #portrait: mentor_neutral
I am currently displaying correctly. 
Now let's try some choices. Choose a fruit:
    +[apple]
        -> chosen("apple")
    +[orange]
        -> chosen("orange")
    +[blueberry]
        -> chosen("blueberry")
        
=== chosen(fruit) ===
You chose {fruit}! Hopefully that is correct.
The End
-> END