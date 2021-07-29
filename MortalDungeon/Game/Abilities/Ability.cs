using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public enum AbilityTypes //basic denominations for skills that can be used for categorizing and sorting
    {
        Empty,
        Move, //a basic movement
        MeleeAttack, //attacks that would be used in close quarters
        RangedAttack, //attacks that can be used from a distance
        DamagingSpell, //an ability that does damage to an enemy (and maybe applies a secondary effect)
        Debuff, //an ability that applies a debuff to the target
        Utility, //an ability that provides utility to 
        Heal, //an ability that heals
        Repositioning //an ability that repositions a unit
    }
    public enum DamageType //main effect (extra effects provided by abilities)
    {
        NonDamaging, //does no damage
        Piercing, //pierces lighter armor, has a higher spread of damage (bleeds, crits)
        Blunt, //lower spread of damage (stuns, incapacitates)
        Mental, //lowers sanity (conversion, insanity, flee)
        Magic, //consistent damage (aoe explosion, forced movement)
        WeakMagic, //low damage but debuffs (dispel, slow, weakness),
        Fire, //high damage but easily resisted (burns)
        Ice, //lowish damage but consistent debuffs (more extreme slow, freeze insta kill, AA shatter) 
    }
    public class Ability
    {
        public AbilityTypes Type = AbilityTypes.Empty;
        public DamageType DamageType;

        public Unit CastingUnit;
        public List<Unit> AffectedUnits = new List<Unit>(); //units that need to be accessed frequently for the ability
        public List<BaseTile> AffectedTiles = new List<BaseTile>(); //tiles that need to be accessed frequently for the ability 

        public List<Unit> Units = new List<Unit>(); //relevant units
        public List<BaseTile> CurrentTiles = new List<BaseTile>(); //the current set of tiles that is being worked with

        public TileMap TileMap;
        public BaseTile SelectedTile;
        public Unit SelectedUnit;

        public int AbilityID => _abilityID;
        protected int _abilityID = _currentAbilityID++;
        protected static int _currentAbilityID = 0;

        public bool HasHoverEffect = false;


        public CombatScene Scene;

        public string Name = "";

        public bool Castable = true; //determines whether this is a behind the scenes ability or a usable ability

        public bool CanTargetAlly = true;
        public bool CanTargetEnemy = true;
        public bool CanTargetSelf = false;
        public bool CanTargetGround = true;
        public bool CanTargetTerrain = false;

        public int EnergyCost = 0;
        public int Range = 0;
        public float Damage = 0;
        public int Duration = 0;

        public Ability() 
        {

        }

        public virtual List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default) 
        {
            return new List<BaseTile>();
        }

        public virtual void AdvanceDuration() 
        {
            Duration--;
            EnactEffect();
        } 
        public virtual void EnactEffect() { } //the actual effect of the skill

        public virtual void OnSelect(CombatScene scene, TileMap currentMap) 
        {
            TileMap = currentMap;
            Scene = scene;
        }

        public virtual void OnTileClicked(TileMap map, BaseTile tile) { }

        public virtual void OnUnitClicked(Unit unit) { }

        public virtual void OnHover() { }
        public virtual void OnHover(BaseTile tile, TileMap map) { }
        public virtual void OnHover(Unit unit) { }

        public virtual void OnRightClick() { }

        public virtual void OnAbilityDeselect() { }

        public virtual void UpdateEnergyCost() { }

        //remove invalid tiles from the list
        protected void TrimTiles(List<BaseTile> validTiles, List<Unit> units, bool trimFog = false) 
        {
            for (int i = 0; i < validTiles.Count; i++)
            {
                if (validTiles[i].TileIndex == CastingUnit.TileMapPosition && !CanTargetSelf)
                {
                    validTiles.RemoveAt(i);
                    i--;
                    continue;
                }

                if (validTiles[i].InFog && !validTiles[i].Explored && trimFog) 
                {
                    validTiles.RemoveAt(i);
                    i--;
                    continue;
                }

                if (!CanTargetEnemy || !CanTargetAlly)
                {
                    for (int j = 0; j < units?.Count; j++)
                    {
                        if (units[j].TileMapPosition == validTiles[i].TileIndex)
                        {
                            if ((!CanTargetAlly && units[j].Team == UnitTeam.Ally) || (!CanTargetEnemy && units[j].Team != UnitTeam.Ally))
                            {
                                validTiles.RemoveAt(i);
                                i--;
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }
}
