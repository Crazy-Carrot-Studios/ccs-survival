using System;
using CCS.Modules.Inventory;
using UnityEngine;

namespace CCS.Modules.Industry
{
    [Serializable]
    public sealed class CCS_IndustryProcessStack
    {
        [SerializeField] private CCS_ItemDefinition itemDefinition;
        [SerializeField] private int quantity = 1;

        public CCS_ItemDefinition ItemDefinition => itemDefinition;

        public int Quantity => quantity < 1 ? 1 : quantity;
    }
}
