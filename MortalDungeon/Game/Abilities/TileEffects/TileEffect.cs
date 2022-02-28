using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Abilities
{
    [Serializable]
    public class TileEffect : ISerializable
    {
        public int Duration = -1;

        public FeaturePoint Location = new FeaturePoint(); 

        [XmlIgnore]
        public Dictionary<int, float> Parameters = new Dictionary<int, float>();
        public DeserializableDictionary<int, float> _parameters = new DeserializableDictionary<int, float>();

        public string _typeName = "";

        public string Identifier = "";

        public TileEffect() { }

        public TileEffect(TileEffect effect)
        {
            Duration = effect.Duration;
            Location = effect.Location;
            Parameters = effect.Parameters;
            _parameters = effect._parameters;
            Identifier = effect.Identifier;

            _typeName = effect._typeName;
        }

        public class TileEffectEventArgs
        {
            public Unit Unit;
            public BaseTile Tile;
            public TileEffectEventArgs(Unit unit, BaseTile tile)
            {
                Unit = unit;
                Tile = tile;
            }
        }


        public virtual void AddedToTile(TilePoint point)
        {
            //do stuff, assign events, add visuals, etc

            Location = point.ToFeaturePoint();
            CreateVisuals();
        }

        public virtual void RemovedFromTile(TilePoint point)
        {
            //clean up any objects here
            RemoveVisuals();
        }

        public virtual void OnRecreated(TilePoint point)
        {
            CreateVisuals();
        }

        public virtual void CreateVisuals()
        {

        }

        public virtual void RemoveVisuals()
        {

        }

        #region events
        public delegate void TileEffectEventHandler(TileEffectEventArgs args);
        public delegate void TileEffectRoundHandler(TilePoint point);

        public event TileEffectEventHandler SteppedOn;
        public event TileEffectEventHandler SteppedOff;
        public event TileEffectEventHandler TurnStart;
        public event TileEffectEventHandler TurnEnd;
        public event TileEffectRoundHandler RoundEnd;
        public event TileEffectRoundHandler RoundStart;

        public virtual void OnSteppedOn(Unit unit, BaseTile tile) 
        {
            SteppedOn?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnSteppedOff(Unit unit, BaseTile tile)
        {
            SteppedOff?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnTurnStart(Unit unit, BaseTile tile)
        {
            TurnStart?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnTurnEnd(Unit unit, BaseTile tile)
        {
            TurnEnd?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnRoundStart(TilePoint point)
        {
            RoundStart?.Invoke(point);
        }

        public virtual void OnRoundEnd(TilePoint point)
        {
            RoundEnd?.Invoke(point);
        }
        #endregion

        public void PrepareForSerialization()
        {
            _parameters = new DeserializableDictionary<int, float>(Parameters);

            _typeName = GetType().Name;
        }

        public void CompleteDeserialization()
        {
            Parameters.Clear();
            _parameters.FillDictionary(Parameters);
        }
    }
}
