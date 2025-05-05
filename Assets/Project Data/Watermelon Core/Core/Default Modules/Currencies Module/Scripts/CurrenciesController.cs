using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class CurrenciesController : MonoBehaviour
    {
        private const string SAVE_UNIQUE_IDENTIFIER = "currency";

        private static CurrenciesController currenciesController;

        [SerializeField] CurrenciesDatabase currenciesDatabase;
        public CurrenciesDatabase CurrenciesDatabase => currenciesDatabase;

        [Space]
        [SerializeField] CurrencyAnimation defaultAnimation;

        public static CurrenciesUIController CurrenciesUIController { get; private set; }

        private static Currency[] currencies;
        public static Currency[] Currencies => currencies;

        private static Dictionary<Currency.Type, int> currenciesLink;

        public string UniqueSaveName => SAVE_UNIQUE_IDENTIFIER;

        // Events
        public static event OnCurrencyAmountChangedCallback OnCurrencyAmountChanged;

        public void Initialise(CurrenciesUIController currenciesUIController)
        {
            currenciesController = this;

            // Get UI component
            CurrenciesUIController = currenciesUIController;

            // Initialsie database
            currenciesDatabase.Initialise();

            // Store active currencies
            currencies = currenciesDatabase.Currencies;

            // Link currencies by the type
            currenciesLink = new Dictionary<Currency.Type, int>();
            for (int i = 0; i < currencies.Length; i++)
            {
                if (!currenciesLink.ContainsKey(currencies[i].CurrencyType))
                {
                    currenciesLink.Add(currencies[i].CurrencyType, i);
                }
                else
                {
                    Debug.LogError(string.Format("[Currency Syste]: Currency with type {0} added to database twice!", currencies[i].CurrencyType));
                }

                var save = SaveController.GetSaveObject<Currency.Save>(SAVE_UNIQUE_IDENTIFIER + ":" + (int)currencies[i].CurrencyType);
                currencies[i].SetSave(save);
            }

            // Initialise ui controller
            CurrenciesUIController.Initialise(currencies);
        }

        public static bool HasAmount(Currency.Type currencyType, int amount)
        {
            return currencies[currenciesLink[currencyType]].Amount >= amount;
        }

        public static int Get(Currency.Type currencyType)
        {
            return currencies[currenciesLink[currencyType]].Amount;
        }

        public static Currency GetCurrency(Currency.Type currencyType)
        {
            return currencies[currenciesLink[currencyType]];
        }

        public static void Set(Currency.Type currencyType, int amount, bool redrawUI = true)
        {
            Currency currency = currencies[currenciesLink[currencyType]];

            currency.Amount = amount;

            // Change save state to required
            SaveController.MarkAsSaveIsRequired();

            // Call redraw currency UI method
            if (redrawUI)
                CurrenciesUIController.RedrawCurrency(currencyType, currency.Amount);

            // Invoke currency change event
            OnCurrencyAmountChanged?.Invoke(currencyType, currency.Amount, 0);
        }

        public static void Add(Currency.Type currencyType, int amount, bool redrawUI = true)
        {
            Currency currency = currencies[currenciesLink[currencyType]];

            currency.Amount += amount;

            // Change save state to required
            SaveController.MarkAsSaveIsRequired();

            // Call redraw currency UI method
            if(redrawUI)
                CurrenciesUIController.RedrawCurrency(currencyType, currency.Amount);

            // Invoke currency change event
            OnCurrencyAmountChanged?.Invoke(currencyType, currency.Amount, amount);
        }

        public static void Substract(Currency.Type currencyType, int amount, bool redrawUI = true)
        {
            Currency currency = currencies[currenciesLink[currencyType]];

            currency.Amount -= amount;

            // Change save state to required
            SaveController.MarkAsSaveIsRequired();

            // Call redraw currency UI method
            if (redrawUI)
                CurrenciesUIController.RedrawCurrency(currencyType, currency.Amount);

            // Invoke currency change event
            OnCurrencyAmountChanged?.Invoke(currencyType, currency.Amount, -amount);
        }

        public static Coroutine PlayCoroutine(IEnumerator routine)
        {
            return currenciesController.StartCoroutine(routine);
        }

        public static CurrencyAnimationCase PlayAnimation(Transform from, Transform to, Currency.Type currencyType, int amount, bool isContinues)
        {
            return currenciesController.defaultAnimation.Play(from, to, currencyType, amount, isContinues);
        }

        public delegate void OnCurrencyAmountChangedCallback(Currency.Type currencyType, int amount, int amountDifference);
    }
}