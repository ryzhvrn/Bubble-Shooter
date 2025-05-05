using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class StoreProduct : ScriptableObject
    {
        [ReadOnlyField]
        [SerializeField] int id;
        public int ID { get { return id; } }

        [SerializeField] string productName;
        public string ProductName => productName;

        [SerializeField] StorePageName page;
        public StorePageName Page => page;

        [SerializeField] StoreProductType type;
        public StoreProductType Type { get { return type; } protected set { type = value; } }

        [SerializeField] BehaviourType behaviourType;
        public BehaviourType BehaviourType { get { return behaviourType; } protected set { behaviourType = value; } }

        [SerializeField] bool isDefault;
        public bool IsDefault => isDefault;

        private List<SpecialField> specialFields = new List<SpecialField>();

        public virtual void Init()
        {
         
        }

        public virtual bool IsAvailable() { return true; }
        public virtual bool IsUnlocked() { return false; }
        public virtual bool CanBeUnlocked() { return false; }
        public virtual void Unlock() { }

        public void SetSpecialFieldValue(string name, string value)
        {
            int index = specialFields.FindIndex(x => x.name == name);
            if (index != -1)
                specialFields[index] = new SpecialField(name, value);
            else
                specialFields.Add(new SpecialField(name, value));
        }

        public T GetSpecialFieldValue<T>(string name, T defaultValue)
        {
            int index = specialFields.FindIndex(x => x.name == name);
            if (index != -1 && !string.IsNullOrEmpty(specialFields[index].value))
            {
                return (T)System.Convert.ChangeType(specialFields[index].value, typeof(T));
            }

            return defaultValue;
        }

        public string GetSpecialFieldValue(string name, string defaultValue)
        {
            int index = specialFields.FindIndex(x => x.name == name);
            if (index != -1 && !string.IsNullOrEmpty(specialFields[index].value))
            {
                return specialFields[index].value;
            }

            return defaultValue;
        }

        [System.Serializable]
        public class SpecialField
        {
            public string name;
            public string value;

            public SpecialField(string name, string value)
            {
                this.name = name;
                this.value = value;
            }
        }
    }

    public enum BehaviourType
    {
        Default = 0,
        Dummy = 1,
        Random = 2,
    }
}