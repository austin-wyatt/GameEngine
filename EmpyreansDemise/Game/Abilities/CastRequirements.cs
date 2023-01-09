using Empyrean.Engine_Classes;
using Empyrean.Game.Items;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Abilities
{
    public enum Comparison
    {
        DontCompare,
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
    }

    public enum ExpendBehavior
    {
        Expend,
        DontExpend,
        Add
    }

    public enum ResourceType
    {
        ResI,
        ResF
    }

    /// <summary>
    /// Provides functionality to use either a ResF or a ResI
    /// </summary>
    public class CombinedResourceCost
    {
        public ResourceType ResourceType;
        public Enum Field;
        public ResourceCost ResourceCost;
    }

    public class ResourceCost
    {
        public float Cost;

        public Comparison Comparison;
        public ExpendBehavior ExpendBehavior = ExpendBehavior.Expend;

        public ResourceCost(float cost, Comparison comparison, ExpendBehavior expendBehavior)
        {
            Cost = cost;

            Comparison = comparison;

            ExpendBehavior = expendBehavior;
        }

        public float GetCost(Unit unit, ResF resource)
        {
            float value = Cost;
            ModifyCostValue(unit, resource, ref value);

            return value;
        }

        public float GetCost(Unit unit, ResI resource)
        {
            float value = Cost;
            ModifyCostValue(unit, resource, ref value);

            return value;
        }

        public bool Check(Unit unit, ResF resource) 
        {
            float value = unit.Info.ResourceManager.GetResource(resource);

            float cost = GetCost(unit, resource);

            return CheckValue(value, cost);
        }

        public bool Check(Unit unit, ResI resource)
        {
            float value = unit.Info.ResourceManager.GetResource(resource);

            float cost = GetCost(unit, resource);

            return CheckValue(value, cost);
        }

        private bool CheckValue(float value, float cost)
        {
            switch (Comparison)
            {
                case Comparison.Equal:
                    return value == cost;
                case Comparison.NotEqual:
                    return value != cost;
                case Comparison.GreaterThan:
                    return value > cost;
                case Comparison.LessThan:
                    return value < cost;
                case Comparison.LessThanOrEqual:
                    return value <= cost;
                case Comparison.GreaterThanOrEqual:
                    return value >= cost;
            }

            return true;
        }

        private void ModifyCostValue(Unit unit, ResF resource, ref float value)
        {
            switch (resource)
            {
                case ResF.ActionEnergy:
                    value = (value + unit.Info.BuffManager.GetValue(BuffEffect.ActionEnergyCostAdditive)) * 
                        unit.Info.BuffManager.GetValue(BuffEffect.ActionEnergyCostMultiplier);
                    return;
                case ResF.MovementEnergy:
                    value = (value + unit.Info.BuffManager.GetValue(BuffEffect.MovementEnergyAdditive)) *
                        unit.Info.BuffManager.GetValue(BuffEffect.MovementEnergyCostMultiplier);
                    return;
                case ResF.Health:
                    value = (value + unit.Info.BuffManager.GetValue(BuffEffect.HealthCostAdditive)) *
                        unit.Info.BuffManager.GetValue(BuffEffect.HealthCostMultiplier);
                    return;
                case ResF.MaxMovementEnergy:
                    value = (value + unit.Info.BuffManager.GetValue(BuffEffect.MaxMovementEnergyAdditive)) *
                        unit.Info.BuffManager.GetValue(BuffEffect.MaxMovementEnergyMultiplier);
                    return;
                case ResF.MaxActionEnergy:
                    value = (value + unit.Info.BuffManager.GetValue(BuffEffect.MaxActionEnergyAdditive)) *
                        unit.Info.BuffManager.GetValue(BuffEffect.MaxActionEnergyMultiplier);
                    return;
            }
        }

        private void ModifyCostValue(Unit unit, ResI resource, ref float value)
        {
            switch (resource)
            {
                case ResI.Stamina:
                    value = (value + unit.Info.BuffManager.GetValue(BuffEffect.StaminaCostAdditive)) *
                        unit.Info.BuffManager.GetValue(BuffEffect.StaminaCostMultiplier);
                    return;
                case ResI.FireAffinity:
                    value = (value + unit.Info.BuffManager.GetValue(BuffEffect.FireAffinityCostAdditive)) *
                        unit.Info.BuffManager.GetValue(BuffEffect.FireAffinityCostMultiplier);
                    return;
            }
        }


        public void Expend(Unit unit, ResI resource)
        {
            float value = GetCost(unit, resource);

            switch (ExpendBehavior)
            {
                case ExpendBehavior.Expend:
                    unit.AddResI(resource, -(int)Math.Ceiling(value));
                    break;
                case ExpendBehavior.Add:
                    unit.AddResI(resource, (int)Math.Floor(value));
                    break;
            }
        }
        public void Expend(Unit unit, ResF resource)
        {
            float value = GetCost(unit, resource);

            switch (ExpendBehavior)
            {
                case ExpendBehavior.Expend:
                    unit.AddResF(resource, -value);
                    break;
                case ExpendBehavior.Add:
                    unit.AddResF(resource, value);
                    break;
            }
        }
    }

    /// <summary>
    /// Checks each member of the ResourceCosts list in order. Only one condition needs to be available to 
    /// cast the ability and only the resource costs of that condition will be expended.
    /// </summary>
    public class VariableResourceCost 
    {
        public List<CombinedResourceCost> ResourceCosts = new List<CombinedResourceCost>();

        public bool Check(Unit unit)
        {
            bool val = false;

            foreach (var cost in ResourceCosts)
            {
                switch (cost.ResourceType)
                {
                    case ResourceType.ResF:
                        val = cost.ResourceCost.Check(unit, (ResF)cost.Field);
                        break;
                    case ResourceType.ResI:
                        val = cost.ResourceCost.Check(unit, (ResI)cost.Field);
                        break;
                }

                if (val)
                    return true;
            }

            return false;
        }

        public void Expend(Unit unit)
        {
            float value;
            foreach (var cost in ResourceCosts)
            {
                switch (cost.ResourceType)
                {
                    case ResourceType.ResF:
                        if (!cost.ResourceCost.Check(unit, (ResF)cost.Field))
                            continue;

                        value = cost.ResourceCost.GetCost(unit, (ResF)cost.Field);

                        switch (cost.ResourceCost.ExpendBehavior)
                        {
                            case ExpendBehavior.Expend:
                                unit.AddResF((ResF)cost.Field, -(int)Math.Ceiling(value));
                                break;
                            case ExpendBehavior.Add:
                                unit.AddResF((ResF)cost.Field, (int)Math.Floor(value));
                                break;
                        }
                        return;
                    case ResourceType.ResI:
                        if (!cost.ResourceCost.Check(unit, (ResI)cost.Field))
                            continue;

                        value = cost.ResourceCost.GetCost(unit, (ResI)cost.Field);

                        switch (cost.ResourceCost.ExpendBehavior)
                        {
                            case ExpendBehavior.Expend:
                                unit.AddResI((ResI)cost.Field, -(int)Math.Ceiling(value));
                                break;
                            case ExpendBehavior.Add:
                                unit.AddResI((ResI)cost.Field, (int)Math.Floor(value));
                                break;
                        }
                        return;
                }
            }
        }
    }

    public class EquipmentRequirement
    {
        //include required tags here
        public ItemTag RequiredTag = ItemTag.None;
        
        public bool CheckUnit(Unit unit)
        {
            return (unit.Info.Equipment.EquippedItemTags & RequiredTag) == RequiredTag;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CastRequirements
    {
        private Dictionary<ResF, ResourceCost> ResourceCostsF = new Dictionary<ResF, ResourceCost>();
        private Dictionary<ResI, ResourceCost> ResourceCostsI = new Dictionary<ResI, ResourceCost>();

        public List<VariableResourceCost> VariableResourceCosts = new List<VariableResourceCost>();

        public EquipmentRequirement EquipmentRequirement = new EquipmentRequirement();

        public bool CheckUnit(Unit unit)
        {
            if (!EquipmentRequirement.CheckUnit(unit)) 
                return false;

            foreach (var kvp in ResourceCostsF)
            {
                if (!kvp.Value.Check(unit, kvp.Key))
                    return false;
            }
            foreach (var kvp in ResourceCostsI)
            {
                if (!kvp.Value.Check(unit, kvp.Key))
                    return false;
            }

            for(int i = 0; i < VariableResourceCosts.Count; i++)
            {
                if (!VariableResourceCosts[i].Check(unit))
                    return false;
            }

            return true;
        }

        public void ExpendResources(Unit unit)
        {
            foreach (var kvp in ResourceCostsF)
            {
                kvp.Value.Expend(unit, kvp.Key);
            }
            foreach (var kvp in ResourceCostsI)
            {
                kvp.Value.Expend(unit, kvp.Key);
            }

            for (int i = 0; i < VariableResourceCosts.Count; i++)
            {
                VariableResourceCosts[i].Expend(unit);
            }
        }

        public float GetResourceCost(Unit unit, ResF resource)
        {
            if(ResourceCostsF.TryGetValue(resource, out var resourceCost))
            {
                return resourceCost.GetCost(unit, resource);
            }

            return 0;
        }

        public float GetResourceCost(Unit unit, ResI resource)
        {
            if (ResourceCostsI.TryGetValue(resource, out var resourceCost))
            {
                return resourceCost.GetCost(unit, resource);
            }

            return 0;
        }

        public void AddResourceCost(ResF resource, float cost, Comparison comparison, ExpendBehavior expendBehavior)
        {
            ResourceCostsF.AddOrSet(resource, new ResourceCost(cost, comparison, expendBehavior));
        }

        public void AddResourceCost(ResI resource, float cost, Comparison comparison, ExpendBehavior expendBehavior)
        {
            ResourceCostsI.AddOrSet(resource, new ResourceCost(cost, comparison, expendBehavior));
        }

        public void RemoveResourceCost(ResF resource)
        {
            ResourceCostsF.Remove(resource);
        }
        public void RemoveResourceCost(ResI resource)
        {
            ResourceCostsI.Remove(resource);
        }
    }
}
