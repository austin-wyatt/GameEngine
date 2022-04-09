using MortalDungeon.Definitions.EventActions;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Events;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Items;
using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Particles;
using MortalDungeon.Game.Save;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Serializers.Abilities;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using MortalDungeon.Game.Units.AIFunctions;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Units
{
    public enum UnitStateInstructions
    {
        GeneratePackName = 500, //Generates a random pack name using the number passed in the Data field as a seed.
        PanToOnLoad = 501,
    }

    public class Unit : GameObject, ILoadableEntity, IEventTarget
    {
        public UnitAI AI;
        public UnitInfo Info;

        public AbilityLoadout AbilityLoadout;

        public VisionGenerator VisionGenerator = new VisionGenerator();

        public bool VisibleThroughFog = false;

        public CombatScene Scene;

        public UnitStatusBar StatusBarComp = null;
        public static int BaseStatusBarZIndex = -10;

        public bool Selectable = false;
        public bool Selected = false;
        public bool Targeted = false;

        public Entity EntityHandle;

        public Vector3 TileOffset = new Vector3();
        public Vector3 SelectionTileOffset = new Vector3();

        public UnitSelectionTile SelectionTile;

        public bool _createStatusBar = false;
        public int _xRotation = 25;
        public string pack_name = "";
        public float _scale = 1;

        public long FeatureID = -1;
        public long ObjectHash = 0;

        public Vector4 Color = new Vector4(1, 1, 1, 1);

        public AnimationSet AnimationSet;

        public int UnitCreationInfoId = 0;

        public ParameterDict UnitParameters = new ParameterDict();

        public Dictionary<string, List<EventAction>> EventActions { get; set; }
        public Dictionary<string, dynamic> EventObjects { get; set; }

        public Unit() { }

        public Unit(CombatScene scene) 
        {
            InitializeUnitInfo();

            Scene = scene;

            Hoverable = true;
            Clickable = true;
            Selectable = true;
        }

        public Unit(CombatScene scene, Spritesheet spritesheet, int spritesheetPos, Vector3 position = default) : base(spritesheet, spritesheetPos, position) 
        {
            InitializeUnitInfo();

            Scene = scene;
        }

        public virtual void InitializeUnitInfo() 
        {
            AI = new UnitAI(this);
            Info = new UnitInfo(this);

            EventActions = new Dictionary<string, List<EventAction>>();
            EventObjects = new Dictionary<string, dynamic>();
        }

        /// <summary>
        /// The calculated tile offset of the unit to place its bottom center point in a tile
        /// </summary>
        private Vector3 _innateTileOffset = new Vector3();

        /// <summary>
        /// Create and add the base object to the unit
        /// </summary>
        public virtual void InitializeVisualComponent()
        {
            //using the AnimationSetName create a base object with those animations
            SetTextureLoaded(false);

            BaseObject obj = CreateBaseObject();
            obj.BaseFrame.CameraPerspective = true;
            obj.BaseFrame.ScaleAll(_scale);
            obj.BaseFrame.RotateX(_xRotation);
            obj.BaseFrame.RotateZ(MathHelper.RadiansToDegrees(Scene._camera.CameraAngle));

            AddBaseObject(obj);

            //calculate innate tile offset
            CalculateInnateTileOffset();

            BaseObject.BaseFrame.SetBaseColor(Color);
        }

        public void CalculateInnateTileOffset()
        {
            BaseObject obj = BaseObject;

            if (obj == null)
                return;

            float minZ = float.MaxValue;

            int stride = obj.BaseFrame.Vertices.Length / obj.BaseFrame.Points;
            for (int i = 0; i < obj.BaseFrame.Vertices.Length; i += stride)
            {
                Vector4 transformedPos = new Vector4(obj.BaseFrame.Vertices[i], obj.BaseFrame.Vertices[i + 1], obj.BaseFrame.Vertices[i + 2], 1);
                transformedPos *= obj.BaseFrame.Scale * obj.BaseFrame.Rotation;

                if (transformedPos.Z < minZ)
                {
                    minZ = transformedPos.Z;
                }
            }

            _innateTileOffset = WindowConstants.ConvertLocalToGlobalCoordinates(new Vector3(0, 0, -minZ));
            _innateTileOffset.Y -= obj.Dimensions.Y / 3;


            CalculateSelectionTileOffset();
            SetPositionOffset(_actualPosition);
        }

        public virtual void EntityLoad(FeaturePoint position, bool placeOnTileMap = true) 
        {
            Scene.Tick += Tick;

            StateChanged += CreateMorsel;

            Position = new Vector3();

            
            SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, 0));
            SetSelectionTileColor();

            SelectionTile.SetRender(false);

            if (VisionGenerator.Team != UnitTeam.Unknown && !Scene.UnitVisionGenerators.Contains(VisionGenerator)) 
            {
                Scene.AddVisionGenerator(VisionGenerator);
            }

            Scene._genericObjects.Add(SelectionTile);

            InitializeVisualComponent();

            if (StatusBarComp == null && _createStatusBar) 
            {
                new UnitStatusBar(this, Scene._camera);
                StatusBarComp.SetWillDisplay(false);
            }

            if (StatusBarComp != null) 
            {
                Scene._unitStatusBlock.AddChild(StatusBarComp, BaseStatusBarZIndex);
            }

            Info.Energy = Info.CurrentEnergy;
            Info.ActionEnergy = Info.CurrentActionEnergy;

            if (placeOnTileMap)
            {
                SetTileMapPosition(TileMapHelpers.GetTile(position));

                SetPositionOffset(Info.TileMapPosition.Position);
            }

            ApplyAbilityLoadout();

            CalculateSelectionTileOffset();

            if (Info.Dead)
            {
                Kill();
            }

            LoadTexture(this);
        }

        private void CalculateSelectionTileOffset()
        {
            if (SelectionTileOffset == null || SelectionTile == null)
                return;

            SelectionTile.UnitOffset.X = 0;
            SelectionTile.UnitOffset.Y = 0;
            SelectionTile.UnitOffset.Z = 0;

            if (SelectionTileOffset.Z != 0)
            {
                SelectionTile.UnitOffset = SelectionTileOffset;
            }

            SelectionTile.UnitOffset.Y -= _innateTileOffset.Y;
            SelectionTile.UnitOffset.Z -= _innateTileOffset.Z - 0.005f;

            SelectionTile.SetPosition(Position);
        }

        public virtual void EntityUnload() 
        {
            CleanUp();

            Scene.RemoveUnit(this);

            StateChanged -= CreateMorsel;

            Info.TileMapPosition = null;

            BaseObjects.Clear();
        }

        public virtual void ApplyAbilityLoadout()
        {
            foreach(var ability in Info.Abilities)
            {
                ability.RemoveAbilityFromUnit();
            }

            Info.Abilities.Clear();

            Move movement = new Move(this);
            movement.EnergyCost = 0.3f;
            Info.Abilities.Add(movement);
            movement.AddAbilityToUnit();

            Info._movementAbility = movement;

            if(AbilityLoadout != null)
            {
                AbilityLoadout.ApplyLoadoutToUnit(this, Info.AbilityVariation);
            }

            AbilitiesUpdated?.Invoke(this);
        }

        public void AddAbility(AbilityLoadoutItem item)
        {
            AbilityLoadout.GetLoadout(Info.AbilityVariation).Add(item);
            ApplyAbilityLoadout();

            OnAbilitiesUpdated();
        }

        public void ReplaceAbility(AbilityLoadoutItem item, AbilityLoadoutItem newItem)
        {
            AbilityLoadout.GetLoadout(Info.AbilityVariation).Replace(item, newItem);
            ApplyAbilityLoadout();

            OnAbilitiesUpdated();
        }

        public void RemoveAbility(AbilityLoadoutItem item)
        {
            AbilityLoadout.GetLoadout(Info.AbilityVariation).Remove(item);
            ApplyAbilityLoadout();

            OnAbilitiesUpdated();
        }

        public void ApplyStateValue(StateIDValuePair state)
        {
            switch (state.Instruction)
            {
                case (int)UnitStateInstructions.GeneratePackName:
                    pack_name = new Random(state.Data).Next().ToString();
                    break;
                case (int)UnitStateInstructions.PanToOnLoad:
                    Scene.SmoothPanCameraToUnit(this, 1);
                    break;
            }



            if (state.Values.Count > 0)
            {
                foreach (var value in state.Values)
                {
                    ApplyStateValue(value);
                }
            }
        }

        public void ApplyUnitParameters(Dictionary<string, string> parameters)
        {
            foreach(var param in parameters)
            {
                UnitParameters.Parameters.TryAdd(param.Key, param.Value);

                FieldInfo info = typeof(Unit).GetField(param.Key);
                if(info == null)
                {
                    info = typeof(UnitInfo).GetField(param.Key);

                    if (info != null)
                    {
                        info.SetValue(Info, Convert.ChangeType(param.Value, info.FieldType));
                        continue;
                    }
                }
                else
                {
                    info.SetValue(this, Convert.ChangeType(param.Value, info.FieldType));
                }
            }
        }

        public override void CleanUp()
        {
            base.CleanUp();
            Scene.Tick -= Tick;

            //remove the objects that are related to the unit but not created by the unit
            Scene._genericObjects.Remove(SelectionTile);
            Scene._unitStatusBlock.RemoveChild(StatusBarComp);

            SelectionTile?.CleanUp();

            StatusBarComp = null;
            SelectionTile = null;

            RemoveFromTile();

            Scene.RemoveVisionGenerator(VisionGenerator);

            Scene.DecollateUnit(this);
        }

        public void RemoveFromTile() 
        {
            if (Info?.TileMapPosition != null)
            {
                UnitPositionManager.RemoveUnitPosition(this, Info.TileMapPosition);
            }
        }

        public override void AddBaseObject(BaseObject obj)
        {
            base.AddBaseObject(obj);
        }

        public Ability GetFirstAbilityOfType(AbilityTypes type)
        {
            foreach (Ability ability in Info.Abilities) 
            {
                if (ability.Type == type)
                    return ability;
            }

            return new Ability();
        }

        public List<Ability> GetAbilitiesOfType(AbilityTypes type)
        {
            List<Ability> abilities = new List<Ability>();

            foreach (Ability ability in Info.Abilities) 
            {
                if (ability.Type == type)
                    abilities.Add(ability);
            }

            return abilities;
        }

        public bool HasAbilityOfType(AbilityTypes type) 
        {
            foreach (Ability ability in Info.Abilities)
            {
                if (ability.Type == type)
                    return true;
            }

            return false;
        }

        public TileMap GetTileMap() 
        {
            return Info?.TileMapPosition?.TilePoint.ParentTileMap;
        }

        public override void SetName(string name)
        {
            base.SetName(name);

            UpdateStatusBarInfo();
        }

        public void UpdateStatusBarInfo() 
        {
            if (StatusBarComp != null)
            {
                StatusBarComp.UpdateInfo();
            }
        }

        public void SetPositionOffset(Vector3 position)
        {
            SetPosition(position + TileOffset + _innateTileOffset);

            _actualPosition = position;
        }


        public Vector3 _actualPosition = new Vector3();
        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            if (StatusBarComp != null) 
            {
                Scene.SyncToRender(() => StatusBarComp.UpdateUnitStatusPosition());
                //StatusBarComp.UpdateUnitStatusPosition();
            }

            if (SelectionTile != null) 
            {
                SelectionTile.SetPosition(position);
            }
        }

        public override void SetRender(bool render)
        {
            base.SetRender(render);

            if (StatusBarComp != null) 
            {
                StatusBarComp.SetWillDisplay(render && !Info.Dead && Scene.DisplayUnitStatuses);
            }

            if (SelectionTile != null)
            {
                if (Targeted && render)
                {
                    SelectionTile.Target();
                }
                else if (Selected && render)
                {
                    SelectionTile.Select();
                }
                else
                {
                    SelectionTile.SetRender(false);
                }
            }
        }

        public virtual void SetTileMapPosition(Tile baseTile) 
        {
            Tile prevTile = null;

            if (Info.TileMapPosition != null)
            {
                prevTile = Info.TileMapPosition;
                UnitPositionManager.RemoveUnitPosition(this, Info.TileMapPosition);
            }
                
            UnitPositionManager.SetUnitPosition(this, baseTile);

            Info.TileMapPosition = baseTile;

            VisionGenerator.SetPosition(baseTile.TilePoint);

            Info.TileMapPosition.OnSteppedOn(this);
            prevTile?.OnSteppedOff(this);

            Scene.OnUnitMoved(this, prevTile);

            UnitMoved?.Invoke(this, prevTile, baseTile);
            StateChanged?.Invoke(this);
        }

        public void ScaleUnit(float x, float y)
        {
            if(BaseObject != null)
            {
                BaseObject.BaseFrame.SetScale(BaseObject.BaseFrame.CurrentScale.X * x, BaseObject.BaseFrame.CurrentScale.Y * y, 1);
                CalculateInnateTileOffset();
            }
        }


        public bool GetPierceShields(DamageType damageType) 
        {
            return damageType switch
            {
                DamageType.Poison => true,
                DamageType.HealthRemoval => true,
                DamageType.Focus => true,
                DamageType.Healing => true,
                _ => false,
            };
        }

        public bool GetAmplifiedByNegativeShields(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Bleed:
                case DamageType.Poison:
                    return false;
                default:
                    return true;
            }
        }

        public delegate void UnitEventHandler(Unit unit);
        public event UnitEventHandler TurnStart;
        public event UnitEventHandler TurnEnd;
        public event UnitEventHandler RoundStart;
        public event UnitEventHandler RoundEnd;

        public event UnitEventHandler Killed;
        public event UnitEventHandler Hurt;
        public event UnitEventHandler Healed;
        public event UnitEventHandler ShieldsHit;
        public event UnitEventHandler AbilitiesUpdated;

        public event UnitEventHandler StateChanged;

        public delegate void UnitMoveEventHandler(Unit unit, Tile prev, Tile current);
        public event UnitMoveEventHandler UnitMoved;

        public void OnStateChanged()
        {
            StateChanged?.Invoke(this);
        }

        protected void CreateMorsel(Unit unit)
        {
            if (Scene.InCombat)
            {
                Scene.CombatState.CreateMorsel(this, Combat.MorselType.Action);
            }
        }

        public void OnTurnStart() 
        {
            if (StatusBarComp != null && StatusBarComp.Render && Scene.InCombat) 
            {
                StatusBarComp.SetIsTurn(true);
            }

            List<Ability> abilitiesToDecay = new List<Ability>();

            lock (Info.Abilities)
            {
                foreach (var ability in Info.Abilities)
                {
                    ability.UsedThisTurn = false;

                    if (ability.DecayCombo())
                    {
                        abilitiesToDecay.Add(ability);
                    }
                }
            }
            

            foreach (var ability in abilitiesToDecay)
            {
                ability.CompleteDecay();
            }

            if(Info.TileMapPosition != null)
            foreach (var effect in TileEffectManager.GetTileEffectsOnTilePoint(Info.TileMapPosition))
            {
                effect.OnTurnStart(this, Info.TileMapPosition);
            }

            TurnStart?.Invoke(this);


            if (AI.ControlType != ControlType.Controlled && Scene.InCombat)
            {
                AIBrain.TakeAITurn(this);
            }
        }

        public void OnTurnEnd()
        {
            if (StatusBarComp != null && StatusBarComp.Render) 
            {
                StatusBarComp.SetIsTurn(false);
            }

            if(Info.TileMapPosition != null)
            foreach (var effect in TileEffectManager.GetTileEffectsOnTilePoint(Info.TileMapPosition))
            {
                effect.OnTurnEnd(this, Info.TileMapPosition);
            }

            TurnEnd?.Invoke(this);
        }

        public void OnRoundStart()
        {
            RoundStart?.Invoke(this);
        }

        public void OnRoundEnd()
        {
            RoundEnd?.Invoke(this);
        }

        public virtual void SetTeam(UnitTeam team) 
        {
            AI.Team = team;

            SetSelectionTileColor();

            UpdateStatusBarInfo();

            VisionGenerator.Team = team;
        }

        /// <summary>
        /// This method is specifically for changing a unit from one team to another as
        /// opposed to the general functionality of the SetTeam method
        /// </summary>
        public void AlterTeam(UnitTeam team)
        {
            UnitTeam prevTeam = AI.Team;

            SetTeam(team);

            if (team != UnitTeam.Unknown)
            {
                VisionManager.CalculateVision(VisionGenerator);
                VisionManager.ConsolidateVision(VisionGenerator.Team);

                if (prevTeam != team)
                {
                    VisionManager.ConsolidateVision(prevTeam);
                }
            }
        }

        public void OnAbilitiesUpdated()
        {
            AbilitiesUpdated?.Invoke(this);
        }

        public void SetSelectionTileColor() 
        {
            if (SelectionTile == null)
                return;

            switch (UnitTeam.PlayerUnits.GetRelation(AI.Team))
            {
                case Relation.Friendly:
                    SelectionTile.SetColor(new Vector4(0, 0.75f, 0, 1));
                    break;
                case Relation.Hostile:
                    SelectionTile.SetColor(new Vector4(0.75f, 0, 0, 1));
                    break;
                case Relation.Neutral:
                    SelectionTile.SetColor(_Colors.Tan);
                    break;
                default:
                    SelectionTile.SetColor(new Vector4(0, 0, 0.75f, 1));
                    break;
            }
        }

        public virtual void SetShields(int shields) 
        {
            Info.CurrentShields = shields;

            if (StatusBarComp != null) 
            {
                StatusBarComp.ShieldBar.SetCurrentShields(Info.CurrentShields);
            }

            if(Scene.Footer.CurrentUnit == this) 
            {
                Scene.Footer.RefreshFooterInfo();
            }

            if (Scene.SideBar.PartyWindow != null && Scene.SideBar.PartyWindow.Parent != null && AI.ControlType == ControlType.Controlled && AI.Team == UnitTeam.PlayerUnits)
            {
                Scene.SideBar.CreatePartyWindowList();
            }
        }

        public void SetHealth(float health) 
        {
            Info.Health = health;

            StatusBarComp.HealthBar.SetHealthPercent(Info.Health / Info.MaxHealth, AI.Team);

            Scene.Footer.RefreshFooterInfo();

            if (Scene.SideBar.PartyWindow != null && Scene.SideBar.PartyWindow.Parent != null && AI.ControlType == ControlType.Controlled && AI.Team == UnitTeam.PlayerUnits)
            {
                Scene.SideBar.CreatePartyWindowList();
            }
        }

        public void Rest() 
        {
            SetHealth(Info.MaxHealth);
            Info.Focus = Info.MaxFocus;
        }

        public struct AppliedDamageReturnValues
        {
            public float DamageBlockedByShields;
            public float ActualDamageDealt;
            public bool KilledEnemy;
            public bool AttackBrokeShield;
        }
        

        public delegate void DamageInstanceEventHandler(Unit unit, DamageInstance instance);
        public event DamageInstanceEventHandler PreDamageInstanceAppliedSource;
        public event DamageInstanceEventHandler PreDamageInstanceAppliedDestination;

        public void OnPreDamageInstanceAppliedSource(DamageInstance instance)
        {
            PreDamageInstanceAppliedSource?.Invoke(this, instance);
        }

        public virtual AppliedDamageReturnValues ApplyDamage(DamageParams damageParams)
        {
            AppliedDamageReturnValues returnVals = new AppliedDamageReturnValues();

            float preShieldDamage = 0;
            float finalDamage = 0;

            DamageInstance instance = damageParams.Instance;
            instance.ApplyDamageBonuses(damageParams.SourceUnit);

            damageParams.SourceUnit?.OnPreDamageInstanceAppliedSource(instance);
            PreDamageInstanceAppliedDestination?.Invoke(this, instance);

            if(!Scene.InCombat && damageParams.SourceUnit.AI.Team.GetRelation(AI.Team) == Relation.Hostile)
            {
                Scene.UnitsInCombat.Add(this);
                Scene.UnitsInCombat.Add(damageParams.SourceUnit);
                Scene.FillInitiativeOrder();
                Scene.InCombat = true;
                Scene.EvaluateCombat(AI.Team);
                Scene.InCombat = false;
                Scene.StartCombat();
            }

            foreach (DamageType type in instance.Damage.Keys)
            {
                Info.BuffManager.GetDamageResistances(type, out float damageAdd, out float damageMult);

                float actualDamage = (instance.Damage[type] + damageAdd) * damageMult;

                if (type == DamageType.Healing)
                {
                    actualDamage *= -1;
                }

                preShieldDamage += actualDamage;

                //shield piercing exceptions should go here
                if (GetPierceShields(type))
                {

                }
                else
                {
                    if (type == DamageType.Piercing)
                    {
                        float piercingDamage = damageParams.Instance.PiercingPercent * actualDamage;

                        actualDamage -= piercingDamage;
                        finalDamage += piercingDamage;
                    }


                    float shieldDamageBlocked;

                    Info.DamageBlockedByShields += actualDamage;

                    if (Info.CurrentShields < 0 && GetAmplifiedByNegativeShields(type))
                    {
                        actualDamage *= 1 + (0.25f * Math.Abs(Info.CurrentShields));
                    }
                    else if (Info.CurrentShields > 0)
                    {
                        shieldDamageBlocked = Info.CurrentShields * Info.ShieldBlock;

                        if (actualDamage - shieldDamageBlocked <= 0)
                        {
                            returnVals.DamageBlockedByShields = actualDamage;
                        }
                        else 
                        {
                            returnVals.DamageBlockedByShields = shieldDamageBlocked;
                        }

                        actualDamage -= shieldDamageBlocked;

                        if (actualDamage <= 0)
                        {
                            actualDamage = 0;

                            OnShieldsHit();
                        }
                    }

                    if (Info.DamageBlockedByShields > Info.ShieldBlock)
                    {
                        Info.CurrentShields--;
                        Info.DamageBlockedByShields = 0;

                        returnVals.AttackBrokeShield = true;
                    }
                }

                //if (type == DamageType.Piercing)
                //{
                //    float piercingDamage = damageParams.Instance.PierceAmount;
                //    //if (damageParams.Ability != null)
                //    //{
                //    //    piercingDamage += damageParams.Ability.Grade;
                //    //}

                //    //if (damageParams.Buff != null)
                //    //{
                //    //    piercingDamage += damageParams.Buff.Grade;
                //    //}

                //    actualDamage += piercingDamage;
                //}

                finalDamage += actualDamage;
            }

            if (preShieldDamage > Info.Stealth.Skill && Info.Stealth.Hiding)
            {
                Info.Stealth.SetHiding(false);
            }

            Info.Health -= finalDamage;

            returnVals.ActualDamageDealt = finalDamage;

            if (finalDamage > 0) 
            {
                OnHurt();

                Scene.EventLog.AddEvent(Name + " has taken " + finalDamage + " damage.", EventSeverity.Info);
            }
            if(finalDamage < 0)
            {
                OnHealed();

                Scene.EventLog.AddEvent(Name + " has healed for " + -finalDamage + " health.", EventSeverity.Info);
            }

            if (Info.Health > Info.MaxHealth) 
            {
                Info.Health = Info.MaxHealth;
            }


            
            SetHealth(Info.Health);
            SetShields(Info.CurrentShields);
            

            if (Info.Health <= 0) 
            {
                Kill();
                returnVals.KilledEnemy = true;
            }

            return returnVals;
        }

        public virtual void Kill() 
        {
            Info.Dead = true;
            OnKill();
        }

        public virtual void Revive()
        {
            Info.Dead = false;
            SetHealth(1);

            SetShields(Info.CurrentShields);

            OnRevive();
        }

        public virtual void OnKill() 
        {
            if (BaseObject != null) 
            {
                BaseObject.SetAnimation(AnimationType.Die);
            }

            Scene.OnUnitKilled(this);

            Sound sound = new Sound(Sounds.Die) { Gain = 0.2f, Pitch = 1 };
            sound.Play();

            Ledgers.OnUnitKilled(this);

            Killed?.Invoke(this);
            StateChanged?.Invoke(this);
        }

        public virtual void OnRevive() 
        {
            BaseObject.SetAnimation(0);

            Ledgers.OnUnitRevived(this);
            StateChanged?.Invoke(this);
        }

        public virtual void OnHurt() 
        {
            Sound sound = new Sound(Sounds.UnitHurt) { Gain = 1f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
            sound.Play();

            var bloodExplosion = new Explosion(Position, new Vector4(1, 0, 0, 1), Explosion.ExplosionParams.Default);
            bloodExplosion.OnFinish = () =>
            {
                Scene._particleGenerators.Remove(bloodExplosion);
            };

            Scene._particleGenerators.Add(bloodExplosion);

            Hurt?.Invoke(this);
            StateChanged?.Invoke(this);
        }

        public virtual void OnHealed()
        {
            Healed?.Invoke(this);
            StateChanged?.Invoke(this);
        }

        public virtual void OnShieldsHit() 
        {
            if (Info.DamageBlockedByShields > Info.ShieldBlock)
            {
                Sound sound = new Sound(Sounds.ArmorHit) { Gain = 1f, Pitch = GlobalRandom.NextFloat(0.6f, 0.7f) };
                sound.Play();
            }
            else 
            {
                Sound sound = new Sound(Sounds.ArmorHit) { Gain = 1f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
                sound.Play();
            }

            var bloodExplosion = new Explosion(Position, new Vector4(0.592f, 0.631f, 0.627f, 1), Explosion.ExplosionParams.Default);
            bloodExplosion.OnFinish = () =>
            {
                Scene._particleGenerators.Remove(bloodExplosion);
            };

            Scene._particleGenerators.Add(bloodExplosion);

            ShieldsHit?.Invoke(this);
            StateChanged?.Invoke(this);
        }

        public override void OnHover()
        {
            if (Hoverable && !Hovered)
            {
                Hovered = true;

                if (StatusBarComp != null && StatusBarComp.Render) 
                {
                    //StatusBarComp.ZIndex = BaseStatusBarZIndex + 1;
                    //Scene.SortUIByZIndex();

                    //StatusBarComp.Parent.Children.Sort();
                }

                //Scene.Footer.UpdateFooterInfo(this);
            }
        }

        public override void OnHoverEnd()
        {
            if (Hovered)
            {
                Hovered = false;
                if (StatusBarComp != null && StatusBarComp.Render)
                {
                    //StatusBarComp.ZIndex = BaseStatusBarZIndex;
                    //Scene.SortUIByZIndex();
                    //StatusBarComp.Parent.Children.Sort();
                }

                base.OnHoverEnd();
            }
        }

        public override void OnTimedHover()
        {
            base.OnTimedHover();
        }

        public override void OnCull()
        {
            if (Cull)
            {
                Scene.DecollateUnit(this);

                if (StatusBarComp != null) 
                {
                    StatusBarComp.Cull = Cull;
                }
            }
            else 
            {
                Scene.CollateUnit(this);

                if (StatusBarComp != null)
                {
                    StatusBarComp.Cull = Cull;
                }
            }
        }

        public void Select() 
        {
            if (SelectionTile == null)
                return;

            SelectionTile.Select();
            Selected = true;
        }

        public void Deselect()
        {
            if (SelectionTile == null)
                return;

            SelectionTile.Deselect();
            Selected = false;
        }

        public void Target() 
        {
            if (SelectionTile == null)
                return;

            SelectionTile.Target();
            Targeted = true;
        }

        public void Untarget() 
        {
            if (SelectionTile == null)
                return;

            SelectionTile.Untarget();
            Targeted = false;
        }

        public override void Tick()
        {
            base.Tick();
        }

        public bool OnTileMap(TileMap map) 
        {
            return Info != null && Info.TileMapPosition != null && Info.TileMapPosition.TileMap == map;
        }

        public virtual BaseObject CreateBaseObject() 
        {
            BaseObject obj = new BaseObject(AnimationSet.BuildAnimationsFromSet(), ObjectID, "", new Vector3(), EnvironmentObjects.BASE_TILE.Bounds);

            obj.BaseFrame.SetBaseColor(Color);

            return obj;
        }
    }
    public struct DamageParams
    {
        public DamageInstance Instance;
        public Ability Ability;
        public Buff Buff;

        public Unit SourceUnit => Buff != null ? Buff.Unit : Ability != null ? Ability.CastingUnit : null;

        public DamageParams(DamageInstance instance, Ability ability = null, Buff buff = null)
        {
            Instance = instance;

            Ability = ability;
            Buff = buff;
        }
    }
}
