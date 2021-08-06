using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Slashing, 
        Piercing, //pierces lighter armor, has a higher spread of damage (bleeds, crits)
        Blunt, //lower spread of damage (stuns, incapacitates)
        Mental, //lowers sanity (conversion, insanity, flee)
        Magic, //consistent damage (aoe explosion, forced movement)
        WeakMagic, //low damage but debuffs (dispel, slow, weakness),
        Fire, //high damage but easily resisted (burns)
        Ice, //lowish damage but consistent debuffs (more extreme slow, freeze insta kill, AA shatter) 

        Bleed, //hurts positive shields but doesn't get amplified by negative shields
        Poison //unaffected by shields
    }
    public class Ability
    {
        public AbilityTypes Type = AbilityTypes.Empty;
        public DamageType DamageType = DamageType.Slashing;

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

        public int Tier = 0;

        public CombatScene Scene;

        public string Name = "";
        public string Description = "";

        public bool Castable = true; //determines whether this is a behind the scenes ability or a usable ability

        public bool CanTargetAlly = true;
        public bool CanTargetEnemy = true;
        public bool CanTargetSelf = false;
        public bool CanTargetGround = true;
        public bool CanTargetTerrain = false;

        public bool CanTargetThroughFog = false;
        public bool CanTargetDeadUnits = false;

        public float EnergyCost = 0;
        public int Range = 0;
        public int CurrentRange = 0;
        public float Damage = 0;
        public int Duration = 0;

        public Icon Icon = new Icon(Icon.DefaultIconSize, Icon.DefaultIcon, Spritesheets.IconSheet);

        public Ability() 
        {

        }

        public virtual List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default) 
        {
            return new List<BaseTile>();
        }

        public Icon GenerateIcon(UIScale scale, bool withBackground = false, Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground, bool showEnergyCost = false) 
        {
            Icon icon = new Icon(Icon, scale, withBackground, backgroundType);

            if (showEnergyCost) 
            {
                UIScale textBoxSize = icon.Size;
                textBoxSize *= 0.333f;

                string energyString = GetEnergyCost().ToString("n1").Replace(".0", "");
                float textScale = 0.05f;


                TextComponent energyCostBox = new TextComponent();
                energyCostBox.SetColor(Colors.UITextBlack);
                energyCostBox.SetText(energyString);
                energyCostBox.SetTextScale(textScale);

                UIScale textDimensions = energyCostBox.GetDimensions();

                if (textDimensions.X > textDimensions.Y) 
                {
                    energyCostBox.SetTextScale((textScale - 0.004f) * textDimensions.Y / textDimensions.X);
                }

                UIBlock energyCostBackground = new UIBlock();
                energyCostBackground.SetColor(Colors.UILightGray);
                energyCostBackground.MultiTextureData.MixTexture = false;

                energyCostBackground.SetSize(textBoxSize);

                energyCostBackground.SetPositionFromAnchor(icon.GetAnchorPosition(UIAnchorPosition.BottomRight), UIAnchorPosition.BottomRight);
                energyCostBox.SetPositionFromAnchor(energyCostBackground.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);

                //energyCostBox.SetPositionFromAnchor(icon.GetAnchorPosition(UIAnchorPosition.BottomRight), UIAnchorPosition.BottomRight);

                icon.AddChild(energyCostBox, 50);
                icon.AddChild(energyCostBackground, 49);

                
            }

            return icon;
        }

        public virtual float GetEnergyCost()
        {
            if (CastingUnit != null)
            {
                return CastingUnit.EnergyCostMultiplier * EnergyCost + CastingUnit.EnergyAddition;
            }

            return EnergyCost;
        }

        public float GetDamage() 
        {
            if (CastingUnit != null)
            {
                return CastingUnit.DamageMultiplier * Damage + CastingUnit.DamageAddition;
            }

            return Damage;
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

            if (Scene.EnergyDisplayBar.CurrentEnergy >= GetEnergyCost())
            {
                AffectedTiles = GetValidTileTargets(currentMap, scene._units);

                TrimTiles(AffectedTiles, Units);

                //AffectedTiles.ForEach(tile =>
                //{
                //    currentMap.SelectTile(tile);
                //});

                

                Scene.EnergyDisplayBar.HoverAmount(GetEnergyCost());
            }
            else
            {
                Scene.DeselectAbility();
            }
        }

        public virtual void OnTileClicked(TileMap map, BaseTile tile) { }

        public virtual bool OnUnitClicked(Unit unit)
        {
            if (CastingUnit.ObjectID == unit.ObjectID && !CanTargetSelf)
            {
                Scene.DeselectAbility();
                return false;
            }

            return true;
        }

        public virtual void OnHover() { }
        public virtual void OnHover(BaseTile tile, TileMap map) { }
        public virtual void OnHover(Unit unit) { }

        public virtual void OnRightClick()
        {
            Scene.DeselectAbility();
        }

        public virtual void OnAbilityDeselect() 
        {
            Scene.EnergyDisplayBar.HoverAmount(0);
            AffectedUnits.ForEach(u => u.Untarget());
        }

        public virtual void UpdateEnergyCost() { }

        /// <summary>
        /// Apply the energy cost and clean up and effects here.
        /// </summary>
        public virtual void OnCast() 
        {
            Scene.DeselectAbility();
            Scene.onAbilityCast(this);
        }

        

        //remove invalid tiles from the list
        protected void TrimTiles(List<BaseTile> validTiles, List<Unit> units, bool trimFog = false) 
        {
            for (int i = 0; i < validTiles.Count; i++)
            {
                if (i < 0)
                    i = 0;

                if (validTiles[i].TileIndex == CastingUnit.TileMapPosition && !CanTargetSelf)
                {
                    validTiles.RemoveAt(i);
                    i--;
                    continue;
                }

                if (CanTargetSelf) 
                {
                    AffectedUnits.Add(CastingUnit);
                    CastingUnit.Target();
                }

                if ((validTiles[i].InFog && !validTiles[i].Explored[CastingUnit.Team] && trimFog) || (validTiles[i].InFog && !CanTargetThroughFog)) 
                {
                    validTiles.RemoveAt(i);
                    i--;
                    continue;
                }

                for (int j = 0; j < units?.Count; j++)
                {
                    if (units[j].TileMapPosition == validTiles[i].TileIndex)
                    {
                        if ((!CanTargetAlly && units[j].Team == UnitTeam.Ally) || (!CanTargetEnemy && units[j].Team == UnitTeam.Enemy))
                        {
                            validTiles.RemoveAt(i);
                            i--;
                            continue;
                        }
                        else if (units[j].Dead && !CanTargetDeadUnits) 
                        {
                            validTiles.RemoveAt(i);
                            i--;
                            continue;
                        }
                        else
                        {
                            AffectedUnits.Add(units[j]);
                            units[j].Target();
                        }
                    }
                }
            }
        }
    }
}
