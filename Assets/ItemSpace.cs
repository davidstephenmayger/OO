using UnityEngine;
using System.Collections;
using System;
namespace ItemSpace {
    public interface IItem
    {
        IEquatable GetSlot();
    }
    public interface IEquipable
    {

    }
    public interface IInventory
    {
        int MaxSize();
        IItem[] GetItems();
        void AddItem(IItem toAdd);
        void DeleteItem(IItem toDelete);
    }
    public interface IBody
    {
        ISlot[] GetSlot();
        IItem[] Equipped();
    }
    public interface IEquals
    {
        public bool Equals(Slot s, Slot y);
    }
    public class Slot
    {
        private enum SlotType { Main, Helmet, Gloves, Boots, Ring, Amulet }
        private SlotType type;
        private Slot(SlotType type)
        {
            this.type = type;
        }
        public bool Equals(Slot slot)
        {
            if (slot.type == this.type)
                return true;
            return false;
        }
        public static Slot MakeMain()
        {
            return new Slot(SlotType.Main);
        }
        public static Slot MakeHelmet()
        {
            return new Slot(SlotType.Helmet);
        }
        public static Slot MakeGloves()
        {
            return new Slot(SlotType.Gloves);
        }
        public static Slot MakeBoots()
        {
            return new Slot(SlotType.Boots);
        }
        public static Slot MakeRing()
        {
            return new Slot(SlotType.Ring);
        }
        public static Slot MakeAmulet()
        {
            return new Slot(SlotType.Amulet);
        }
    }
}
