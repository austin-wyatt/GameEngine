using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.TextHandling;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static MortalDungeon.Game.Units.Disposition;
using Icon = MortalDungeon.Engine_Classes.UIComponents.Icon;

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
        BuffDefensive,
        BuffOffensive,
        Utility, //an ability that provides utility
        Heal, //an ability that heals
        Repositioning, //an ability that repositions a unit
        Summoning,
        Passive
    }

    /// <summary>
    /// The classes of abilities. These will range from monster types to the standard classes such as "warrior"
    /// </summary>
    public enum AbilityClass
    {
        Unknown,
        Skeleton,
        Bandit
    }

    /// <summary>
    /// The casting method of the ability. This will be used to determine if the ability can be cast based off of 
    /// the unit's current status effects.
    /// </summary>
    public enum CastingMethod
    {
        Base = 1, //base value
        Vocal = 2, //incanting a spell, screaming at someone, etc
        BruteForce = 4, //hitting someone with a club
        PhysicalDexterity = 8, //picking a lock, bandaging wounds, etc
        Weapon = 16, //stabbing someone with a sword or shooting a bow
        Unarmed = 32, //punch, kick, bite, etc
        Magic = 64, //a magical attack of any variety
        Intelligence = 128, //activating a complicated device
        Passive = 256, //an always active ability
        Movement = 512, //walking, flying, jumping, etc
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
        Poison, //unaffected by shields
        Focus, //damage caused by using the meditation intrinsic ability
        HealthRemoval, //dota health removal
        Healing
    }

    public enum AbilityContext 
    {
        SkipEnergyCost,
        SkipIconAnimation
    }
    public class Ability
    {
        public AbilityTypes Type = AbilityTypes.Empty;
        public DamageType DamageType = DamageType.NonDamaging;
        public AbilityClass AbilityClass = AbilityClass.Unknown;
        public CastingMethod CastingMethod = CastingMethod.Base;



        public int Grade = 1;
        /// <summary>
        /// Denotes abilities that do not have charges and that act as a base ability in an ability tree.
        /// </summary>
        public bool BasicAbility = false;
        public AbilityTreeType AbilityTreeType = AbilityTreeType.None;
        public int NodeID = -1;

        public Unit CastingUnit;

        public List<Unit> AffectedUnits = new List<Unit>(); //units that need to be accessed frequently for the ability

        public List<BaseTile> AffectedTiles = new List<BaseTile>(); //tiles that need to be accessed frequently for the ability 

        public List<Unit> Units = new List<Unit>(); //relevant units

        public List<BaseTile> CurrentTiles = new List<BaseTile>(); //the current set of tiles that is being worked with

        public TileMap TileMap => CastingUnit.GetTileMap();

        public BaseTile SelectedTile;

        public Unit SelectedUnit;

        public ContextManager<AbilityContext> Context = new ContextManager<AbilityContext>();

        #region Combo ability variables
        public bool IsComboAbility = false;

        public Ability Previous;
        public Ability Next;

        public bool DecayToFirst = false;
        public int ComboAdvanceCost = 1;
        public int ComboDecayCost = 2;

        public int ComboDecayCount = 0;
        public int ComboAdvanceCount = 0;

        public bool ShouldDecay = true;
        #endregion

        public int AbilityID => _abilityID;
        protected int _abilityID = _currentAbilityID++;
        protected static int _currentAbilityID = 0;

        public bool HasHoverEffect = false;

        public CombatScene Scene => CastingUnit.Scene;

        public string Name = "";
        public string Description = "";

        public bool Castable = true; //determines whether this is a behind the scenes ability or a usable ability
        public bool MustCast = false;

        public bool CanTargetSelf = false;
        public bool CanTargetGround = true;
        public bool CanTargetTerrain = false;

        public int ChargesLostOnUse = 1;
        public const int ChargeRechargeActionCost = 1;

        public UnitSearchParams UnitTargetParams = new UnitSearchParams()
        {
            Dead = UnitCheckEnum.False,
            IsFriendly = UnitCheckEnum.SoftTrue,
            IsHostile = UnitCheckEnum.SoftTrue,
            IsNeutral = UnitCheckEnum.SoftTrue,
        };

        public bool BreakStealth = true;

        public bool CanTargetThroughFog = false;

        public float EnergyCost = 0;
        public int ActionCost = 1;

        public float Range = 0;
        public int MinRange;
        public int CurrentRange = 0;
        public float Damage = 0;
        public int Duration = 0;
        public float Sound = 0;

        public int MaxCharges = 3;
        public int Charges = 3;

        public float ChargeRechargeCost = 10;

        public bool UsedThisTurn = false;
        public bool OneUsePerTurn = true;

        public Icon Icon = new Icon(Icon.DefaultIconSize, Icon.DefaultIcon, Spritesheets.IconSheet);

        public Ability()
        {

        }

        public virtual List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        {
            AffectedUnits.Clear();
            AffectedTiles.Clear();

            return new List<BaseTile>();
        }

        public Icon GenerateIcon(UIScale scale, bool withBackground = false, Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground,
            bool showEnergyCost = false, Icon passedIcon = null, string hotkey = null, bool showCharges = false)
        {
            Icon icon;
            if (passedIcon == null)
            {
                icon = new Icon(Icon, scale, withBackground, backgroundType);
            }
            else
            {
                icon = new Icon(passedIcon, scale, withBackground, backgroundType);
            }

            if (showEnergyCost && ActionCost > 0 && Type != AbilityTypes.Passive)
            {
                UIScale textBoxSize = icon.Size;
                textBoxSize *= 0.3333f;

                float energyCost = GetEnergyCost();
                float actionCost = GetActionEnergyCost();

                string energyString = (actionCost >= energyCost ? actionCost : energyCost).ToString("n1").Replace(".0", "");

                float textScale = 0.05f;


                Text energyCostBox = new Text(energyString, Text.DEFAULT_FONT, 16, Brushes.White);
                energyCostBox.SetTextScale(textScale);
                

                UIScale textDimensions = energyCostBox.GetDimensions();

                if (textDimensions.X > textDimensions.Y)
                {
                    energyCostBox.SetTextScale((textScale - 0.004f) * textDimensions.Y / textDimensions.X);
                }

                UIBlock energyCostBackground;

                if (ActionCost > 0)
                {
                    energyCostBackground = new UIBlock(default, null, default, (int)IconSheetIcons.Channel, true, false, Spritesheets.IconSheet);
                    energyCostBackground.SetColor(_Colors.White);
                }
                else 
                {
                    energyCostBackground = new UIBlock();
                    energyCostBackground.SetColor(_Colors.UILightGray);
                }


                energyCostBackground.MultiTextureData.MixTexture = false;

                energyCostBackground.SetSize(textBoxSize);

                energyCostBackground.SetPositionFromAnchor(icon.GetAnchorPosition(UIAnchorPosition.BottomRight), UIAnchorPosition.BottomRight);
                energyCostBox.SetPositionFromAnchor(energyCostBackground.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);

                //energyCostBox.SetPositionFromAnchor(icon.GetAnchorPosition(UIAnchorPosition.BottomRight), UIAnchorPosition.BottomRight);

                energyCostBackground.AddChild(energyCostBox);

                icon.AddChild(energyCostBackground, 49);

                energyCostBackground.RenderAfterParent = true;
                Vector3 newPos = new Vector3(energyCostBackground.Position.X, energyCostBackground.Position.Y, icon.Position.Z);
                energyCostBackground.SetPosition(newPos);
                //energyCostBackground.SetAllInline(0);
            }

            if (showCharges && Type != AbilityTypes.Passive) 
            {
                if(MaxCharges > 0)
                {
                    icon.AddChargeDisplay(this);
                }
                
                if(ActionCost > 0)
                {
                    icon.AddActionCost(this);
                }
            }

            if (IsComboAbility && Type != AbilityTypes.Passive) 
            {
                icon.AddComboIndicator(this);
            }

            if (hotkey != null && Type != AbilityTypes.Passive)
            {
                UIScale textBoxSize = icon.Size;
                textBoxSize *= 0.2f;

                float textScale = 0.07f;


                Text hotkeyBox = new Text(hotkey, Text.DEFAULT_FONT, 16, Brushes.White);
                hotkeyBox.SetTextScale(textScale);

                UIScale textDimensions = hotkeyBox.GetDimensions();

                if (textDimensions.X > textDimensions.Y)
                {
                    hotkeyBox.SetTextScale((textScale - 0.004f) * textDimensions.Y / textDimensions.X);
                }

                UIBlock hotkeyBackground = new UIBlock();
                hotkeyBackground.SetColor(_Colors.UITextBlack);
                hotkeyBackground.MultiTextureData.MixTexture = false;

                hotkeyBackground.SetSize(textBoxSize);

                hotkeyBackground.SetPositionFromAnchor(icon.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
                hotkeyBox.SetPositionFromAnchor(hotkeyBackground.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);

                //energyCostBox.SetPositionFromAnchor(icon.GetAnchorPosition(UIAnchorPosition.BottomRight), UIAnchorPosition.BottomRight);

                icon.AddChild(hotkeyBox, 50);
                icon.AddChild(hotkeyBackground, 49);
            }

            return icon;
        }

        public virtual float GetEnergyCost()
        {
            if (CastingUnit != null)
            {
                return CastingUnit.Info.EnergyCostMultiplier * EnergyCost + CastingUnit.Info.EnergyAddition;
            }

            return EnergyCost;
        }

        public virtual float GetActionEnergyCost()
        {
            if (CastingUnit != null)
            {
                return ActionCost + CastingUnit.Info.ActionEnergyAddition;
            }

            return ActionCost;
        }

        public int GetMaxCharges() 
        {
            if(Previous == null) 
            {
                return MaxCharges;
            }
            else 
            {
                return Previous.GetMaxCharges();
            }
        }

        public int GetCharges()
        {
            if (Previous == null)
            {
                return Charges;
            }
            else
            {
                return Previous.GetCharges();
            }
        }

        public float GetDamage()
        {
            float damage;
            if (CastingUnit != null)
            {
                damage = CastingUnit.Info.DamageMultiplier * Damage + CastingUnit.Info.DamageAddition;
            }
            else 
            {
                damage = Damage;
            }

            return Damage;
        }

        public virtual bool UnitInRange(Unit unit, BaseTile position = null)
        {
            if (CanTargetSelf && unit == CastingUnit)
                return true;

            return false;
        }

        /// <summary>
        /// Will return false if a unit is closer than the minimum range
        /// </summary>
        public virtual bool UnitUnderRange(Unit unit)
        {
            return MinRange > TileMap.GetDistanceBetweenPoints(unit.Info.TileMapPosition.TilePoint, CastingUnit.Info.TileMapPosition.TilePoint);
        }

        public virtual void AdvanceDuration()
        {
            Duration--;
            EnactEffect();
        }
        public virtual void EnactEffect()
        {
            Scene.SetAbilityInProgress(true);
        } //the actual effect of the skill

        public virtual void OnSelect(CombatScene scene, TileMap currentMap)
        {
            if (CanCast())
            {
                AffectedTiles = GetValidTileTargets(currentMap, scene._units);

                TrimTiles(AffectedTiles, Units);

                Scene.EnergyDisplayBar.HoverAmount(GetEnergyCost());
                Scene.ActionEnergyBar.HoverAmount(GetActionEnergyCost());
            }
            else
            {
                Scene.DeselectAbility();
            }
        }

        public bool CanCast() 
        {
            return CastingUnit.Info.Energy >= GetEnergyCost() && CastingUnit.Info.ActionEnergy >= GetActionEnergyCost() && HasSufficientCharges()
                && CastingUnit.Info.CanUseAbility(this) && !(Scene.InCombat && UsedThisTurn);
        }


        public void TargetAffectedUnits()
        {
            if (CastingUnit.AI.ControlType == ControlType.Controlled)
            {
                AffectedUnits.ForEach(u =>
                {
                    u.Target();
                });
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
            Scene.ActionEnergyBar.HoverAmount(0);
            AffectedUnits.ForEach(u => u.Untarget());

            AffectedUnits.Clear();
            AffectedTiles.Clear();
        }

        public bool GetEnergyIsSufficient() 
        {
            return GetEnergyCost() <= CastingUnit.Info.Energy && GetActionEnergyCost() <= CastingUnit.Info.ActionEnergy;
        }

        public virtual void UpdateEnergyCost() { }

        public virtual void ApplyEnergyCost()
        {
            if (Context.GetFlag(AbilityContext.SkipEnergyCost))
            {
                Context.SetFlag(AbilityContext.SkipEnergyCost, false);
                return;
            }

            if (CastingUnit.AI.ControlType == ControlType.Controlled)
            {
                Scene.EnergyDisplayBar.HoverAmount(0);
                Scene.ActionEnergyBar.HoverAmount(0);

                float energyCost = GetEnergyCost();
                float actionEnergyCost = GetActionEnergyCost();

                Scene.EnergyDisplayBar.AddEnergy(-energyCost);
                Scene.ActionEnergyBar.AddEnergy(-actionEnergyCost);

                CastingUnit.Info.Energy -= energyCost;
                CastingUnit.Info.ActionEnergy -= actionEnergyCost;
            }
            else
            {
                float energyCost = GetEnergyCost();
                float actionEnergyCost = GetActionEnergyCost();

                CastingUnit.Info.Energy -= energyCost;
                CastingUnit.Info.ActionEnergy -= actionEnergyCost;
            }
        }

        public virtual void ApplyChargeCost(bool fromLast = false) 
        {
            if (Previous == null)
            {
                if (GetMaxCharges() > 0 && ChargesLostOnUse > 0)
                {
                    if(fromLast || !IsComboAbility) 
                    {
                        Charges -= ChargesLostOnUse;
                    }
                }
            }
            else
            {
                Previous.ApplyChargeCost(Next == null || fromLast);
            }
        }

        public virtual void RestoreCharges(int amount) 
        {
            if (Previous == null)
            {
                Charges += amount;
            }
            else
            {
                Previous.RestoreCharges(amount);
            }
        }

        public virtual bool HasSufficientCharges() 
        {
            int maxCharges = GetMaxCharges();
            return maxCharges == 0 || GetCharges() > 0;
        }
        public void ApplyChargeRechargeCost() 
        {
            if (Scene.InCombat)
            {
                CastingUnit.Info.ActionEnergy -= ChargeRechargeActionCost;
            }

            if (CastingUnit.Info.Focus >= ChargeRechargeCost)
            {
                CastingUnit.Info.Focus -= ChargeRechargeCost;
            }
            else
            {
                float damageNum = ChargeRechargeCost - CastingUnit.Info.Focus;
                CastingUnit.Info.Focus = 0;

                DamageInstance damage = new DamageInstance();
                damage.Damage.Add(DamageType.Focus, damageNum);

                CastingUnit.ApplyDamage(new Unit.DamageParams(damage));
            }
        }

        public bool CanRecharge()
        {
            return CastingUnit.Info.Health + CastingUnit.Info.Focus >= ChargeRechargeCost && CastingUnit.Info.ActionEnergy >= ChargeRechargeActionCost;
        }

        /// <summary>
        /// Apply the energy cost and clean up and effects here.
        /// </summary>
        public virtual void OnCast()
        {
            if (Scene.InCombat)
            {
                ApplyEnergyCost();
            }

            ApplyChargeCost();

            Scene.DeselectAbility();
            Scene.OnAbilityCast(this);
        }

        public virtual void OnAICast()
        {
            if (Scene.InCombat)
            {
                ApplyEnergyCost();
            }

            ApplyChargeCost();
        }

        public void Casted()
        {
            if (CastingUnit.AI.ControlType == ControlType.Controlled)
            {
                OnCast();
            }
            else
            {
                OnAICast();
            }

            CreateIconHoverEffect();
            CreateTemporaryVision();

            if (Scene.InCombat)
                UsedThisTurn = OneUsePerTurn;

            if (BreakStealth && CastingUnit.Info.Stealth.Hiding)
            {
                CastingUnit.Info.Stealth.SetHiding(false);
            }

            AffectedUnits.Clear();
            AffectedTiles.Clear();

            TileMap.Controller.DeselectTiles();
        }

        public void CreateTemporaryVision()
        {
            if (SelectedUnit == null)
                return;

            TemporaryVision vision = new TemporaryVision();

            vision.TargetUnit = CastingUnit;
            vision.TickTarget = TickDurationTarget.OnUnitTurnStart;
            vision.Team = SelectedUnit.AI.Team;
            vision.TilesToReveal = SelectedUnit.Info.TileMapPosition.TileMap.GetVisionInRadius(CastingUnit.Info.Point, 1);
            vision.Duration = 1;

            Scene.TemporaryVision.Add(vision);
        }

        public void CreateIconHoverEffect(Icon passedIcon = null)
        {
            if (Context.GetFlag(AbilityContext.SkipIconAnimation))
            {
                Context.SetFlag(AbilityContext.SkipIconAnimation, false);
                return;
            }


            Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground;

            switch (CastingUnit.AI.Team)
            {
                case UnitTeam.PlayerUnits:
                    backgroundType = Icon.BackgroundType.BuffBackground;
                    break;
                case UnitTeam.BadGuys:
                    backgroundType = Icon.BackgroundType.DebuffBackground;
                    break;
            }

            Icon icon = GenerateIcon(new UIScale(0.5f * WindowConstants.AspectRatio, 0.5f), true, backgroundType, false, passedIcon);

            Vector3 pos;
            if (SelectedUnit != null)
            {
                pos = SelectedUnit.Position + new Vector3(0, -400, 0.3f);
            }
            else if (SelectedTile != null)
            {
                pos = SelectedTile.Position + new Vector3(0, -400, 0.3f);
            }
            else
            {
                pos = CastingUnit.Position + new Vector3(0, -400, 0.3f);
            }

            UIHelpers.CreateIconHoverEffect(icon, Scene, pos);
        }

        /// <summary>
        /// Called once all skill effects have been resolved and another skill can be used.
        /// </summary>
        public virtual void EffectEnded()
        {
            //since this method can be called from the main thread via a ticking property animation we need to spawn a task to avoid sleeping the main thread
            Task.Run(() =>
            {
                if (CastingUnit.AI.ControlType != ControlType.Controlled && Type != AbilityTypes.Move)
                {
                    Thread.Sleep(200);
                }
                EffectEndedAction?.Invoke();

                Scene.SetAbilityInProgress(false);

                if (IsComboAbility)
                {
                    AdvanceCombo();
                }

                Scene.Footer.UpdateFooterInfo(Scene.Footer._currentUnit);
            });
        }
        public Action EffectEndedAction = null;

        //remove invalid tiles from the list
        protected void TrimTiles(List<BaseTile> validTiles, List<Unit> units, bool trimFog = false, int minRange = 0, List<Unit> validUnits = null)
        {
            HashSet<BaseTile> validTilesSet = validTiles.ToHashSet();


            for (int i = 0; i < validTiles.Count; i++)
            {
                if (i < 0)
                    i = 0;

                if (validTiles[i].TilePoint == CastingUnit.Info.TileMapPosition && !CanTargetSelf)
                {
                    validTilesSet.Remove(validTiles[i]);
                    validTiles.RemoveAt(i);
                    i--;
                    continue;
                }

                if (minRange > 0 && minRange >= TileMap.GetDistanceBetweenPoints(validTiles[i].TilePoint, CastingUnit.Info.TileMapPosition.TilePoint))
                {
                    validTilesSet.Remove(validTiles[i]);
                    validTiles.RemoveAt(i);
                    i--;
                    continue;
                }

                if (CanTargetSelf)
                {
                    AffectedUnits.Add(CastingUnit);
                }


                if ((!validTiles[i].InVision(CastingUnit.AI.Team) && !validTiles[i].Explored[CastingUnit.AI.Team] && trimFog) || (!validTiles[i].InVision(CastingUnit.AI.Team) && !CanTargetThroughFog))
                {
                    validTilesSet.Remove(validTiles[i]);
                    validTiles.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            for (int j = 0; j < units?.Count; j++)
            {
                BaseTile tile = units[j].Info.TileMapPosition;

                if (units[j] == CastingUnit)
                {
                    if (!CanTargetSelf)
                    {
                        validTiles.Remove(tile);
                        validTilesSet.Remove(tile);
                        continue;
                    }
                    else
                    {
                        AffectedUnits.Add(units[j]);
                        continue;
                    }
                }

                if (!validTilesSet.Contains(tile))
                {
                    validTiles.Remove(tile);
                    validTilesSet.Remove(tile);
                    continue;
                }


                if (!UnitTargetParams.CheckUnit(units[j], CastingUnit)) 
                {
                    validTiles.Remove(tile);
                    validTilesSet.Remove(tile);
                    continue;
                }
                else if (!units[j].Info.Stealth.Revealed[CastingUnit.AI.Team])
                {
                    validTiles.Remove(tile);
                    validTilesSet.Remove(tile);
                    continue;
                }
                else if ((tile.InFog[CastingUnit.AI.Team] && !tile.Explored[CastingUnit.AI.Team] && trimFog) || (tile.InFog[CastingUnit.AI.Team] && !CanTargetThroughFog))
                {
                    validTiles.Remove(tile);
                    validTilesSet.Remove(tile);
                    continue;
                }
                else
                {
                    if(validUnits != null)
                    {
                        validUnits.Add(units[j]);
                    }
                    AffectedUnits.Add(units[j]);
                }
            }
        }

        protected void TrimUnits(List<Unit> units, bool trimFog = false, int minRange = 0)
        {
            for (int j = 0; j < units?.Count; j++)
            {
                BaseTile tile = units[j].Info.TileMapPosition;

                if (units[j] == CastingUnit && !CanTargetSelf)
                {
                    continue;
                }

                if (minRange > 0 && minRange >= TileMap.GetDistanceBetweenPoints(tile.TilePoint, CastingUnit.Info.TileMapPosition.TilePoint))
                {
                    continue;
                }

                if (!UnitTargetParams.CheckUnit(units[j], CastingUnit))
                {
                    continue;
                }
                else if (!units[j].Info.Stealth.Revealed[CastingUnit.AI.Team])
                {
                    continue;
                }
                else if ((tile.InFog[CastingUnit.AI.Team] && !tile.Explored[CastingUnit.AI.Team] && trimFog) || (tile.InFog[CastingUnit.AI.Team] && !CanTargetThroughFog))
                {
                    continue;
                }
                else
                {
                    AffectedUnits.Add(units[j]);
                }
            }
        }


        protected string _description = "ability description text";
        public virtual Tooltip GenerateTooltip()
        {
            string body = _description;

            body += $"\n\n";

            body += GetDamageInstance().GetTooltipStrings(CastingUnit);

            body += $"{GetEnergyCost()} Energy\n";
            body += $"{MinRange}-{Range} Range";

            if (IsComboAbility)
            {
                if (Next != null)
                {
                    body += $"\n\nNext in combo: {Next.Name}";
                }
                else if (Previous != null) 
                {
                    Ability prev = Previous;
                    while (prev.Previous != null) prev = prev.Previous;

                    body += $"\n\nNext in combo: {prev.Name}";
                }
            }

            Tooltip tooltip = UIHelpers.GenerateTooltipWithHeader(Name, body);

            return tooltip;
        }

        public virtual bool DecayCombo(int decayAmount = 1)
        {
            if (IsComboAbility && Previous != null && ShouldDecay)
            {
                ComboDecayCount += decayAmount;

                if (ComboDecayCount >= ComboDecayCost)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void CompleteDecay()
        {
            if (Previous != null)
            {
                if (DecayToFirst) ReturnToFirst();
                else SwapOutAbility(Previous);
            }
        }

        public virtual void AdvanceCombo(int advanceAmount = 1)
        {
            if (IsComboAbility)
            {
                ComboAdvanceCount += advanceAmount;

                if (ComboAdvanceCount >= ComboAdvanceCost)
                {
                    CompleteAdvance();
                }
            }
        }

        public virtual void CompleteAdvance()
        {
            if (Next != null)
            {
                SwapOutAbility(Next);
            }
            else
            {
                ReturnToFirst();
            }
        }

        public virtual void OnSwappedTo()
        {
            ComboDecayCount = 0;
            ComboAdvanceCount = 0;

            if (Scene.Footer != null && Scene.Footer._currentUnit == CastingUnit)
            {
                Scene.Footer.UpdateFooterInfo(CastingUnit);
            }
        }

        public virtual void SwapOutAbility(Ability ability)
        {
            lock (CastingUnit.Info.Abilities)
            {
                CastingUnit.Info.Abilities.Replace(this, ability);
                ability.OnSwappedTo();
            }
        }

        public virtual void ReturnToFirst()
        {
            Ability ability = this;

            while (ability.Previous != null)
            {
                ability = ability.Previous;

                if (ability == this)
                {
                    SwapOutAbility(ability.Previous);
                    return;
                }
            }

            SwapOutAbility(ability);
        }

        public virtual void AddCombo(Ability next, Ability previous, bool shouldDecay = true)
        {
            Next = next;
            Previous = previous;

            if (previous != null)
            {
                previous.Next = this;
                previous.IsComboAbility = true;

                previous.ShouldDecay = shouldDecay;
            }

            if (next != null)
            {
                next.Previous = this;
                next.IsComboAbility = true;

                next.ShouldDecay = shouldDecay;
            }

            IsComboAbility = true;

            ShouldDecay = shouldDecay;
        }

        public int GetComboSize() 
        {
            Ability root = this;

            while (root.Previous != null)
            {
                root = root.Previous;
            }

            int count = 1;
            while (root.Next != null) 
            {
                root = root.Next;
                count++;
            }

            return count;
        }

        public int GetPositionInCombo() 
        {
            int count = 0;

            Ability root = this;

            while (root.Previous != null)
            {
                root = root.Previous;
                count++;
            }

            return count;
        }

        public virtual DamageInstance GetDamageInstance() 
        {
            return new DamageInstance();
        }

        public void ApplyBuffDamageInstanceModifications(DamageInstance instance) 
        {
            if (CastingUnit != null) 
            {
                foreach (var buff in CastingUnit.Info.Buffs) 
                {
                    buff.ModifyDamageInstance(instance, this);
                }
            }
        }

        public virtual UnitAIAction GetAction(List<Unit> unitsInCombat)
        {
            var action = new UnitAIAction(CastingUnit, AIAction.MoveCloser);

            return action;
        }

        public override bool Equals(object obj)
        {
            return obj is Ability ability &&
                   _abilityID == ability._abilityID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_abilityID);
        }
    }
}
