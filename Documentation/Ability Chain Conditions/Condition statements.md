This document will outline the structure required by condition statements.



T = true

F = false

&& = and the preceding and proceeding statements

|| = or the preceding and proceeding statements

! = invert the proceeding statement

() = enclose a statement containing booleans

{} = enclose a statement containing a request for game data



#### Game data requests

The specific syntax for each game data request will differ based on the request. 

The basic layout for a game date request is "Source Category Field". 

* "Source" denotes where the data is coming from
  * Examples of sources: CastingUnit, TargetUnit, Scene, StateValue, etc
* "Category" provides contextual information for where to retrieve data from a source
  * Examples of categories: "CastingUnit ResF", "Scene EntityCount", etc
* "Field" points to a specific item in a category
  * Example of field: "CastingUnit ResF ActionEnergy"

A full game data request could look something like:

"{CastingUnit ResF ActionEnergy}"

This request would replace the game data request in the condition string with the result of the request. 

Ie. "{CastingUnit ResF ActionEnergy} > 2" might become "3 > 2" which would then resolve to "T"



