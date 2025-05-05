using UnityEngine;
using static Watermelon.Currency;

namespace Watermelon
{
    [System.Serializable]
    public class Currency
    {
        [SerializeField] Type currencyType;
        public Type CurrencyType => currencyType;

        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [SerializeField] GameObject model;
        public GameObject Model => model;

        [SerializeField] GameObject pickableObject;
        public GameObject PickableObject => pickableObject;

        [SerializeField] GameObject storageObject;
        public GameObject StorageObject => storageObject;

        [SerializeField] AudioClip pickUpSound;
        public AudioClip PickUpSound => pickUpSound;

        [SerializeField] bool displayAlways = false;
        public bool DisplayAlways => displayAlways;

        public int Amount { get => save.Amount; set => save.Amount = value; }

        public string AmountFormatted => CurrenciesHelper.Format(CurrenciesController.Get(currencyType));

        private Pool pool;
        public Pool Pool => pool;

        private Pool pickablePool;
        public Pool PickablePool => pickablePool;

        private Pool storagePool;
        public Pool StoragePool => storagePool;

        private Save save;

        public void Initialise()
        {
            pool = new Pool(new PoolSettings(currencyType.ToString(), model, 1, true));
            storagePool = new Pool(new PoolSettings(storageObject.name, storageObject, 0, true));
            pickablePool = new Pool(new PoolSettings(pickableObject.name, pickableObject, 0, true));
        }

        public void SetSave(Save save)
        {
            this.save = save;
        }

        // DO NOT CHANGE ORDER OF THE CURRENCIES AFTER RELEASE. IT CAN BRAKE THE SAVES OF THE GAME!
        public enum Type
        {
            Wood = 0,
            Planks = 1,
            Coal = 2,
            Stone = 3,
            Crystal = 4,
            Money = 5
        }

        [System.Serializable]
        public class Save : ISaveObject
        {
            [SerializeField] int amount;
            public int Amount { get => amount; set => amount = value; }

            public void Flush()
            {

            }
        }
    }
}