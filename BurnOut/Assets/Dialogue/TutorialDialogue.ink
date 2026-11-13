-> move

=== move ===
#name: Mentor #portrait: mentor_neutral #canvas: default
Welcome to the firehouse! 
So you think you're cut out for the job, eh?
* [Sure thing!]
Sure thing! #name: Rookie #portrait: player_neutral
#name: Mentor #portrait: mentor_neutral
    Excellent, so why fire fighting?
    -> q2
* [Yeah]
Yeah #name: Rookie #portrait: player_neutral
#name: Mentor #portrait: mentor_neutral
    So why fire fighting?
    -> q2
* [Perchance...]
Perchance... #name: Rookie #portrait: player_neutral
#name: Mentor #portrait: mentor_neutral
Right, so why fire fighting?
    -> q2
    
=== q2 === 
* [I hated my last job...]
I hated my last job... #name: Rookie #portrait: player_neutral
* [I want to try something new!]
I want to try something new! #name: Rookie #portrait: player_neutral
* [Need to exercise these legs somehow]
Need to exercise these legs somehow #name: Rookie #portrait: player_neutral
- -> r2

=== r2 ===
#name: Mentor #portrait: mentor_neutral
Don't we all...
#name: Rookie #portrait: player_neutral
...
#name: Mentor #portrait: mentor_neutral
Well, don't just stand there, get a move on then and go pick up that water gun!
-> DONE

=== shoot ===
#name: Mentor #portrait: mentor_neutral #canvas: default
Go through that door and knock over some of those targets!

Fire spreads. Lucky for you, this is just cardboard, BWAHAHHA

#name: Rookie #portrait: player_neutral
I'll try my best...
-> DONE

=== reload ===
// after running out of water
#name: Mentor #portrait: mentor_neutral #canvas: default
Ha! Out of water already? 
Haul that fire hydrant over here and fill 'er up!
-> DONE

=== targets50 ===
#name: Mentor #portrait: mentor_neutral #canvas: pop_up
Good enough, I suppose...
Through that door will be your final challenge...
-> DONE

=== targets75 ===
#name: Mentor #portrait: mentor_neutral #canvas: pop_up
Not too shabby!
Through that door will be your final challenge...
-> DONE

=== targets100 ===
#name: Mentor #portrait: mentor_neutral #canvas: pop_up
Great work! 
Through that door will be your final challenge...
-> DONE

=== chair === 
#name: Mentor #portrait: mentor_neutral #canvas: default
Here is your final challenge...
Put that fire out!

#name: Rookie #portrait: player_neutral
On it!
-> DONE

=== leave === 
#name: Mentor #portrait: mentor_neutral #canvas: default
Aha! Now we're cooking with fire!
Well, water 
Anyways! You're hired!

// alarm goes off (maybe)
#name: Mentor #portrait: mentor_neutral #canvas: default
Oh would you look at that!
Get to the truck, we've got a fire to put out!
-> DONE

