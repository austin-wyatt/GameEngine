using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.Text;
using Empyrean.Engine_Classes.TextHandling;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities.AIImplementations;
using Empyrean.Game.Map;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Game.Units.AIFunctions;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Icon = Empyrean.Engine_Classes.UIComponents.Icon;

namespace Empyrean.Game.Abilities
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
        Roguery,
        Spider,


        Item_Normal,
        Item_Magical,
        Item_Divine
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
        Sight = 1024,

        Innate = 4096, //an ability that a unit can do naturally
        Equipped = 8192, //an equipped item's ability
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
        Healing,
    }

    public enum AbilityContext 
    {
        SkipEnergyCost,
        SkipIconAnimation,
    }
    public class Ability
    {
        public AbilityTypes Type = AbilityTypes.Empty;
        public DamageType DamageType = DamageType.NonDamaging;
        public AbilityClass AbilityClass = AbilityClass.Unknown;
        public CastingMethod CastingMethod = CastingMethod.Base;


        public int Grade = 1;

        public AbilityTreeType AbilityTreeType = AbilityTreeType.None;
        public int NodeID = -1;

        public Unit CastingUnit;

        public TileMap TileMap => CastingUnit.GetTileMap();


        public ContextManager<AbilityContext> Context = new ContextManager<AbilityContext>();

        public EffectManager EffectManager;
        public SelectionInfo SelectionInfo;
        public AITargetSelection AITargetSelection = new AITargetSelection();

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

        /// <summary>
        /// Indicates that this ability is for movement and shouldn't be considered for general
        /// use during a turn.
        /// </summary>
        public bool IsForMovement = false;

        public CombatScene Scene => CastingUnit.Scene;

        public TextEntry Name = TextEntry.EMPTY_ENTRY;
        public TextEntry Description = TextEntry.EMPTY_ENTRY;

        public bool Castable = true; //determines whether this is a behind the scenes ability or a usable ability
        public bool MustCast = false;

        public int ChargesLostOnUse = 1;
        public const int ChargeRechargeActionCost = 1;

        
        public bool BreakStealth = true;

        //public float EnergyCost = 0;
        //public int ActionCost = 1;

        public CastRequirements CastRequirements = new CastRequirements();

        public virtual float Range { get; set; }
        public int MinRange;
        public int CurrentRange = 0;
        public float Sound = 0;

        public int MaxCharges = 3;
        public int Charges = 3;

        public float ChargeRechargeCost = 10;

        public bool UsedThisTurn = false;
        public bool OneUsePerTurn = false;

        public AnimationSet AnimationSet = new AnimationSet();

        public Ability()
        {
            EffectManager = new EffectManager(this);
        }

        public Ability(Unit unit)
        {
            CastingUnit = unit;
            EffectManager = new EffectManager(this);
            SelectionInfo = new SelectionInfo(this);
        }

        public Icon GetIcon()
        {
            if(AnimationSet != null)
            {
                return new Icon(Icon.DefaultIconSize, AnimationSet.BuildAnimationsFromSet(), true);
            }

            return null;
        }

        public virtual void AddAbilityToUnit(bool fromLoad = false)
        {
            ApplyPassives();
        }

        public virtual void RemoveAbilityFromUnit(bool fromLoad = false)
        {
            RemovePassives();
        }

        public virtual void GetValidTileTargets(TileMap tileMap, out List<Tile> affectedTiles, out List<Unit> affectedUnits, 
            List<Unit> units = default, Tile position = null)
        {
            affectedTiles = new List<Tile>();
            affectedUnits = new List<Unit>();
        }

        /// <summary>
        /// Returns whether the destination point would be a valid tile regardless of whether it is or isn't <para/>
        /// This is intended simply to check things like whether the destination is in range and there is an 
        /// unobstructed line (if applicable).
        /// </summary>
        public virtual bool GetPositionValid(TilePoint sourcePos, TilePoint destinationPos)
        {
            return true;
        }

        public Icon GenerateIcon(UIScale scale, bool withBackground = false, Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground,
            bool showEnergyCost = false, Icon passedIcon = null, string hotkey = null, bool showCharges = false, float hotkeyTextScale = 1f)
        {
            Icon icon;
            if (passedIcon == null)
            {
                icon = new Icon(GetIcon(), scale, withBackground, backgroundType);
            }
            else
            {
                icon = new Icon(passedIcon, scale, withBackground, backgroundType);
            }

            if (showEnergyCost && GetCost(ResF.ActionEnergy) > 0 && Type != AbilityTypes.Passive)
            {
                UIScale textBoxSize = icon.Size;
                textBoxSize *= 0.3333f;

                float energyCost = GetCost(ResF.MovementEnergy);
                float actionCost = GetCost(ResF.ActionEnergy);

                string energyString = (actionCost >= energyCost ? actionCost : energyCost).ToString("n1").Replace(".0", "");

                float textScale = 1f;


                Text_Drawing energyCostBox = new Text_Drawing(energyString, Text_Drawing.DEFAULT_FONT, 16, Brushes.White);
                energyCostBox.SetTextScale(textScale);
                

                UIScale textDimensions = energyCostBox.GetDimensions();

                if (textDimensions.X > textDimensions.Y)
                {
                    energyCostBox.SetTextScale((textScale - 0.004f) * textDimensions.Y / textDimensions.X);
                }

                UIBlock energyCostBackground;

                if (actionCost > 0)
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
                
                if(GetCost(ResF.ActionEnergy) > 0)
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
                UIScale textBoxSize = new UIScale(hotkeyTextScale * 0.05f, hotkeyTextScale * 0.05f);

                TextString hotkeyBox = new TextString(new FontInfo(UIManager.DEFAULT_FONT_INFO_16, 10), TextAlignment.Center)
                {
                    TextColor = _Colors.White,
                    VerticalAlignment = VerticalAlignment.Center
                };
                hotkeyBox.SetText(hotkey);

                UIBlock hotkeyBackground = new UIBlock();
                hotkeyBackground.SetColor(_Colors.UITextBlack);
                hotkeyBackground.MultiTextureData.MixTexture = false;

                hotkeyBackground.SetSize(textBoxSize);

                hotkeyBackground.SetPositionFromAnchor(icon.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
                hotkeyBox.SetPosition(hotkeyBackground.GetAnchorPosition(UIAnchorPosition.Center));

                hotkeyBackground.AddTextString(hotkeyBox);
                icon.BaseComponent.AddChild(hotkeyBackground, 49);
            }

            return icon;
        }

        public float GetCost(ResF resource)
        {
            return CastRequirements.GetResourceCost(CastingUnit, resource);
        }
        public float GetCost(ResI resource)
        {
            return CastRequirements.GetResourceCost(CastingUnit, resource);
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

        public void BeginEffect()
        {
            Scene.SetAbilityInProgress(true);
            EffectEndedAsync = new TaskCompletionSource<bool>();
        }

        public virtual void EnactEffect()
        {
            Task.Run(EffectManager.EnactEffect);
        } 

        public virtual void OnSelect(CombatScene scene, TileMap currentMap)
        {
            if (CanCast())
            {
                SelectionInfo.SelectAbility();

                //GetValidTileTargets(currentMap, out var affectedTiles, out var affectedUnits, scene._units);

                //AffectedTiles = affectedTiles;
                //AffectedUnits = affectedUnits;

                //TargetAffectedUnits();

                //Scene.EnergyDisplayBar.HoverAmount(GetCost(ResF.MovementEnergy));
                //Scene.ActionEnergyBar.HoverAmount(GetCost(ResF.ActionEnergy));
            }
            else
            {
                Scene.DeselectAbility();
            }
        }

        public bool CanCast() 
        {
            return CastRequirements.CheckUnit(CastingUnit) && HasSufficientCharges() && 
                CastingUnit.Info.CanUseAbility(this) && !(Scene.InCombat && UsedThisTurn) && 
                Type != AbilityTypes.Passive;
        }


        //public void TargetAffectedUnits()
        //{
        //    if (CastingUnit.AI.ControlType == ControlType.Controlled)
        //    {
        //        AffectedUnits.ForEach(u =>
        //        {
        //            u.Target();
        //        });
        //    }
        //}

        public virtual void OnTileClicked(TileMap map, Tile tile) 
        {
            SelectionInfo.TileClicked(tile);
        }

        public virtual bool OnUnitClicked(Unit unit)
        {
            return SelectionInfo.UnitClicked(unit);
        }

        public virtual void OnHover() { }
        public virtual void OnHover(Tile tile, TileMap map) 
        {
            SelectionInfo.TileHovered(tile);
        }
        public virtual void OnHover(Unit unit) { }

        public virtual void OnRightClick()
        {
            SelectionInfo.OnRightClick();
            
        }

        public virtual void OnAbilityDeselect()
        {
            SelectionInfo.DeselectAbility();

            //Scene.EnergyDisplayBar.HoverAmount(0);
            //Scene.ActionEnergyBar.HoverAmount(0);
            //AffectedUnits.ForEach(u => u.Untarget());

            //AffectedUnits.Clear();
            //AffectedTiles.Clear();
        }

        public virtual void UpdateEnergyCost() { }

        public virtual void ApplyEnergyCost()
        {
            if (Context.GetFlag(AbilityContext.SkipEnergyCost))
            {
                Context.SetFlag(AbilityContext.SkipEnergyCost, false);
                return;
            }

            if (CastingUnit.AI.GetControlType() == ControlType.Controlled)
            {
                Scene.EnergyDisplayBar.HoverAmount(0);
                Scene.ActionEnergyBar.HoverAmount(0);

                float energyCost = GetCost(ResF.MovementEnergy);
                float actionEnergyCost = GetCost(ResF.ActionEnergy);

                Scene.EnergyDisplayBar.AddEnergy(-energyCost);
                Scene.ActionEnergyBar.AddEnergy(-actionEnergyCost);

                CastRequirements.ExpendResources(CastingUnit);
            }
            else
            {
                CastRequirements.ExpendResources(CastingUnit);
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
                CastingUnit.AddResF(ResF.ActionEnergy, -ChargeRechargeActionCost);
            }

            //if (CastingUnit.Info.Focus >= ChargeRechargeCost)
            //{
            //    CastingUnit.Info.Focus -= ChargeRechargeCost;
            //}
            //else
            //{
            //    float damageNum = ChargeRechargeCost - CastingUnit.Info.Focus;
            //    CastingUnit.Info.Focus = 0;

            //    DamageInstance damage = new DamageInstance();
            //    damage.Damage.Add(DamageType.Focus, damageNum);

            //    CastingUnit.ApplyDamage(new DamageParams(damage));
            //}
        }

        public bool CanRecharge()
        {
            return CastingUnit.GetResF(ResF.Health) + CastingUnit.GetResF(ResF.ActionEnergy) >= ChargeRechargeActionCost;
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
            if (CastingUnit.AI.GetControlType() == ControlType.Controlled)
            {
                OnCast();
            }
            else
            {
                OnAICast();
            }

            //CreateIconHoverEffect();
            CreateTemporaryVision();

            if (Scene.InCombat)
                UsedThisTurn = OneUsePerTurn;

            if (BreakStealth && CastingUnit.Info.Stealth.Hiding)
            {
                CastingUnit.Info.Stealth.SetHiding(false);
            }
        }

        public void CreateTemporaryVision()
        {
            if (SelectionInfo.SelectedUnits.Count > 0)
                return;

            

            for(int j = 0; j < SelectionInfo.SelectedUnits.Count; j++)
            {
                TemporaryVision vision = new TemporaryVision();

                vision.TargetUnit = CastingUnit;
                vision.TickTarget = TickDurationTarget.OnUnitTurnStart;
                vision.Team = SelectionInfo.SelectedUnits[j].AI.GetTeam();
                vision.TilesToReveal = SelectionInfo.SelectedUnits[j].Info.TileMapPosition.TileMap.GetVisionInRadius(CastingUnit.Info.Point, 1);
                vision.Duration = 1;

                for (int i = 0; i < vision.TilesToReveal.Count; i++)
                {
                    vision.AffectedMaps.Add(vision.TilesToReveal[i].TileMap);
                }

                Scene.TemporaryVision.Add(vision);

                void updateTemporaryVision()
                {
                    EffectEndedAction -= updateTemporaryVision;
                    Scene.UpdateTemporaryVision();
                }

                EffectEndedAction += updateTemporaryVision;
            }
        }

        public void CreateIconHoverEffect(Icon passedIcon = null)
        {
            if (Context.GetFlag(AbilityContext.SkipIconAnimation))
            {
                Context.SetFlag(AbilityContext.SkipIconAnimation, false);
                return;
            }


            Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground;

            switch (CastingUnit.AI.GetTeam())
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
            if (SelectionInfo.SelectedUnits.Count > 0)
            {
                pos = SelectionInfo.SelectedUnits[0]._position + new Vector3(0, -400, 0.3f);
            }
            else if (SelectionInfo.SelectedTiles.Count > 0)
            {
                pos = SelectionInfo.SelectedTiles[0]._position + new Vector3(0, -400, 0.3f);
            }
            else
            {
                pos = CastingUnit._position + new Vector3(0, -400, 0.3f);
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
                if (CastingUnit.AI.GetControlType() != ControlType.Controlled && Type != AbilityTypes.Move)
                {
                    Thread.Sleep(200);
                }

                Scene.SetAbilityInProgress(false);

                if (IsComboAbility)
                {
                    AdvanceCombo();
                }

                if (RefreshFooterOnFinish)
                {
                    Scene.Footer.RefreshFooterInfo();
                }

                EffectEndedAction?.Invoke();

                EffectEndedAsync.TrySetResult(true);
            });
        }

        public TaskCompletionSource<bool> EffectEndedAsync;
        public Action EffectEndedAction = null;
        public bool RefreshFooterOnFinish = true;

        public virtual Tooltip GenerateTooltip()
        {
            var lines = Description.ToString().Split('\n');
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > 0)
                {
                    builder.Append(UIHelpers.WrapString(lines[i], 50));
                }

                builder.Append('\n');
            }

            string body = builder.ToString();

            body += $"\n\n";

            body += GetDamageInstance().GetTooltipStrings(CastingUnit);

            body += $"{GetCost(ResF.MovementEnergy)} Energy\n";
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

            Tooltip tooltip = UIHelpers.GenerateTooltipWithHeader(Name.ToString(), body);

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

            ApplyPassives();

            if (Scene.Footer != null && Scene.Footer.CurrentUnit == CastingUnit)
            {
                Scene.Footer.RefreshFooterInfo();
            }
        }

        public virtual void SwapOutAbility(Ability ability)
        {
            lock (CastingUnit.Info.Abilities)
            {
                CastingUnit.Info.Abilities.Replace(this, ability);
                RemovePassives();

                CastingUnit.OnAbilitiesUpdated();

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

        /// <summary>
        /// Apply any passive effects to a unit with this ability equipped
        /// </summary>
        public virtual void ApplyPassives()
        {

        }

        /// <summary>
        /// Remove any passive effects to a unit with this ability equipped
        /// </summary>
        public virtual void RemovePassives()
        {

        }

        public virtual DamageInstance GetDamageInstance() 
        {
            return new DamageInstance();
        }

        /// <summary>
        /// Should generally only be redefined by template abilities. 
        /// Template abilities should then define a set of parameters that can be defined
        /// by the ability to modify the desired targets. <para/>
        /// Template abilities should generally define their own class implementation of IAIAction as well.
        /// </summary>
        public virtual List<IAIAction> GetDesiredTargets()
        {
            return new List<IAIAction>();
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
