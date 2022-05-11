# Conditionals

## CONSTANTS
AND
  ands the preceding and following statements together

OR
  ors the preceding and following statements together

NOT
  inverses the true/false evaluation of the statement following the keyword

T
  evaluates to true

F
  evaluates to false

()
  statements inside of parenthesis are evaluated first

## Quest (int)
completed 
  true if the quest with the passed id has been completed

available
  true if the quest with the passed id is not in progress or completed
  (perhaps also allow quest ids to contain their own 
  additional available conditionals)

inProgress
  true if the quest with the passed id is currently active

## Dialogue (int)
outcome (int)
  true if the outcome for the passed dialogue id has been ledgered

## Item
inInventory (int)
  true if the item with the passed id is in the player's inventory

equipped (int)
  true if any unit in the player's party has an item with this id equipped

goldLessThan (int)
  true if the player party's gold is less than the passed amount

goldGreaterThan (int)
  true if the player party's gold in greater than the passed amount

goldEqual (int)
  true if the player party's gold is equal to the passed amount

## Scene
inCombat
  true if the current scene is in combat