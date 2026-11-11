-> main

=== main ===
#name: Mentor #portrait: mentor_neutral #canvas: default
Welcome to the firehouse! 
So you think you're cut out for the job, eh?
-> r1

=== r1 ===
#name: Rookie #portrait: player_neutral
* [Sure thing!]
Sure thing!
#name: Mentor #portrait: mentor_neutral
    Excellent, so why fire fighting?
    -> q2
* [Yeah]
Yeah
#name: Mentor #portrait: mentor_neutral
    So why fire fighting?
    -> q2
* [Perchance...]
Perchance...
#name: Mentor #portrait: mentor_neutral
    Right, so why fire fighting?
    -> q2
    
=== q2 === 
#name: Rookie #portrait: player_neutral
* [I hated my last job...]
I hated my last job...
* [I want to try something new!]
I want to try something new!
* [Need to exercise these legs somehow]
Need to exercise these legs somehow
- -> r2

=== r2 ===
#name: Mentor #portrait: mentor_neutral
Don't we all...
#name: Rookie #portrait: player_neutral
* [...]
#name: Mentor #portrait: mentor_neutral
    Well, don't just stand there, get a move on then and go pick up that water gun!
-> DONE