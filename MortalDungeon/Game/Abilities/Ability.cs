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
using System.Threading;
using System.Threading.Tasks;
using static MortalDungeon.Game.Units.Disposition;

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
        Buff, //an ability that applies a buff to the target
        Utility, //an ability that provides utility
        Heal, //an ability that heals
        Repositioning, //an ability that repositions a unit
        Summoning
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

    public enum AbilityContext 
    {
        SkipEnergyCost,
        SkipIconAnimation
    }
    public class Ability
    {
        public AbilityTypes Type = AbilityTypes.Empty;
        public DamageType DamageType = DamageType.NonDamaging;
        public int Grade = 1;

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
        #endregion

        public int AbilityID => _abilityID;
        protected int _abilityID = _currentAbilityID++;
        protected static int _currentAbilityID = 0;

        public bool HasHoverEffect = false;

        public int Tier = 0;

        public CombatScene Scene => CastingUnit.Scene;

        public string Name = "";
        public string Description = "";

        public bool Castable = true; //determines whether this is a behind the scenes ability or a usable ability

        public bool CanTargetSelf = false;
        public bool CanTargetGround = true;
        public bool CanTargetTerrain = false;

        public UnitSearchParams UnitTargetParams = new UnitSearchParams()
        {
            Dead = CheckEnum.False,
            IsFriendly = CheckEnum.SoftTrue,
            IsHostile = CheckEnum.SoftTrue,
            IsNeutral = CheckEnum.SoftTrue,
        };

        public bool BreakStealth = true;

        public bool CanTargetThroughFog = false;

        public float EnergyCost = 0;
        public float Range = 0;
        public int MinRange;
        public int CurrentRange = 0;
        public float Damage = 0;
        public int Duration = 0;
        public float Sound = 0;

        public Icon Icon = new Icon(Icon.DefaultIconSize, Icon.DefaultIcon, Spritesheets.IconSheet);

        public Ability()
        {

        }

        public virtual List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            return new List<BaseTile>();
        }

        public Icon GenerateIcon(UIScale scale, bool withBackground = false, Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground, bool showEnergyCost = false, Icon passedIcon = null, string hotkey = null)
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

            if (hotkey != null)
            {
                UIScale textBoxSize = icon.Size;
                textBoxSize *= 0.2f;

                float textScale = 0.03f;


                TextComponent hotkeyBox = new TextComponent();
                hotkeyBox.SetColor(Colors.UILightGray);
                hotkeyBox.SetText(hotkey);
                hotkeyBox.SetTextScale(textScale);

                UIScale textDimensions = hotkeyBox.GetDimensions();

                if (textDimensions.X > textDimensions.Y)
                {
                    hotkeyBox.SetTextScale((textScale - 0.004f) * textDimensions.Y / textDimensions.X);
                }

                UIBlock hotkeyBackground = new UIBlock();
                hotkeyBackground.SetColor(Colors.UITextBlack);
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

        public float GetDamage()
        {
            if (CastingUnit != null)
            {
                return CastingUnit.Info.DamageMultiplier * Damage + CastingUnit.Info.DamageAddition;
            }

            return Damage;
        }

        public virtual bool UnitInRange(Unit unit, BaseTile position = null)
        {
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
            Scene.AbilityInProgress = true;
        } //the actual effect of the skill

        public virtual void OnSelect(CombatScene scene, TileMap currentMap)
        {
            if (Scene.EnergyDisplayBar.CurrentEnergy >= GetEnergyCost())
            {
                AffectedTiles = GetValidTileTargets(currentMap, scene._units);

                TrimTiles(AffectedTiles, Units);

                Scene.EnergyDisplayBar.HoverAmount(GetEnergyCost());
            }
            else
            {
                Scene.DeselectAbility();
            }
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
            AffectedUnits.ForEach(u => u.Untarget());

            AffectedUnits.Clear();
            AffectedTiles.Clear();
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

                float energyCost = GetEnergyCost();

                Scene.EnergyDisplayBar.AddEnergy(-energyCost);
            }
            else
            {
                float energyCost = GetEnergyCost();

                CastingUnit.Info.Energy -= energyCost;
            }
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

            Scene.DeselectAbility();
            Scene.OnAbilityCast(this);
        }

        public virtual void OnAICast()
        {
            if (Scene.InCombat)
            {
                ApplyEnergyCost();
            }
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

            if (BreakStealth && CastingUnit.Info.Stealth.Hiding)
            {
                CastingUnit.Info.Stealth.SetHiding(false);
            }

            AffectedUnits.Clear();
            AffectedTiles.Clear();
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


            icon.SetCameraPerspective(true);
            if (SelectedUnit != null)
            {
                icon.SetPosition(SelectedUnit.Position + new Vector3(0, -400, 0.3f));
            }
            else if (SelectedTile != null)
            {
                icon.SetPosition(SelectedTile.Position + new Vector3(0, -400, 0.3f));
            }
            else
            {
                icon.SetPosition(CastingUnit.Position + new Vector3(0, -400, 0.3f));
            }

            Scene._genericObjects.Add(icon);

            PropertyAnimation anim = new PropertyAnimation(icon.BaseObject.BaseFrame);

            float xMovement = (float)(new Random().NextDouble() - 1) * 10f;

            for (int i = 0; i < 50; i++)
            {
                Keyframe frame = new Keyframe(i * 2)
                {
                    Action = () =>
                    {
                        icon.SetPosition(icon.Position + new Vector3(xMovement, -10, 0.015f));
                        icon.SetColor(icon.BaseObject.BaseFrame.BaseColor - new Vector4(0, 0, 0, 0.02f));
                    }
                };

                anim.Keyframes.Add(frame);
            }

            icon.AddPropertyAnimation(anim);
            anim.Play();

            anim.OnFinish = () =>
            {
                icon.RemovePropertyAnimation(anim.AnimationID);
                Scene._genericObjects.Remove(icon);
            };
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
                    Thread.Sleep(750);
                }
                EffectEndedAction?.Invoke();

                Scene.AbilityInProgress = false;

                if (IsComboAbility)
                {
                    AdvanceCombo();
                }

                Scene.Footer.UpdateFooterInfo(Scene.Footer._currentUnit);
            });
        }
        public Action EffectEndedAction = null;

        //remove invalid tiles from the list
        protected void TrimTiles(List<BaseTile> validTiles, List<Unit> units, bool trimFog = false, int minRange = 0)
        {
            for (int i = 0; i < validTiles.Count; i++)
            {
                if (i < 0)
                    i = 0;

                if (validTiles[i].TilePoint == CastingUnit.Info.TileMapPosition && !CanTargetSelf)
                {
                    validTiles.RemoveAt(i);
                    i--;
                    continue;
                }

                if (minRange > 0 && minRange >= TileMap.GetDistanceBetweenPoints(validTiles[i].TilePoint, CastingUnit.Info.TileMapPosition.TilePoint))
                {
                    validTiles.RemoveAt(i);
                    i--;
                    continue;
                }

                if (CanTargetSelf)
                {
                    AffectedUnits.Add(CastingUnit);
                }

                if ((validTiles[i].InFog[CastingUnit.AI.Team] && !validTiles[i].Explored[CastingUnit.AI.Team] && trimFog) || (validTiles[i].InFog[CastingUnit.AI.Team] && !CanTargetThroughFog))
                {
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
                        validTiles.Remove(units[j].Info.TileMapPosition);
                        continue;
                    }
                    else 
                    {
                        AffectedUnits.Add(units[j]);
                        continue;
                    }
                }

                if (!validTiles.Contains(units[j].Info.TileMapPosition))
                {
                    validTiles.Remove(units[j].Info.TileMapPosition);
                    continue;
                }


                if (!UnitTargetParams.CheckUnit(units[j], CastingUnit)) 
                {
                    validTiles.Remove(units[j].Info.TileMapPosition);
                    continue;
                }
                else if (!units[j].Info.Stealth.Revealed[CastingUnit.AI.Team])
                {
                    validTiles.Remove(units[j].Info.TileMapPosition);
                    continue;
                }
                else if ((tile.InFog[CastingUnit.AI.Team] && !tile.Explored[CastingUnit.AI.Team] && trimFog) || (tile.InFog[CastingUnit.AI.Team] && !CanTargetThroughFog))
                {
                    validTiles.Remove(units[j].Info.TileMapPosition);
                    continue;
                }
                else
                {
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
            //if (DamageType != DamageType.NonDamaging) body += $"{Damage} {DamageType} damage\n";
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
            if (IsComboAbility && Previous != null)
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

        public virtual void AddCombo(Ability next, Ability previous)
        {
            Next = next;
            Previous = previous;

            if (previous != null)
            {
                previous.Next = this;
                previous.IsComboAbility = true;
            }

            if (next != null)
            {
                next.Previous = this;
                next.IsComboAbility = true;
            }

            IsComboAbility = true;
        }

        public virtual DamageInstance GetDamageInstance() 
        {
            return new DamageInstance();
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
