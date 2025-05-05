using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class CurrenciesUIController : MonoBehaviour
    {
        private const float DISALBE_PANEL_IN_SECONDS = 10.0f;

        [SerializeField] GameObject panelObject;
        [SerializeField] Transform parentTrasnform;

        private Pool panelPool;

        private Dictionary<Currency.Type, CurrencyUI> activePanelsUI;

        private void Awake()
        {
            panelPool = new Pool(new PoolSettings("Currency Panel", panelObject, 3, true, parentTrasnform));
        }

        public void Initialise(Currency[] currencies)
        {
            activePanelsUI = new Dictionary<Currency.Type, CurrencyUI>();
            for (int i = 0; i < currencies.Length; i++)
            {
                if (currencies[i].DisplayAlways)
                {
                    GameObject currencyObject = panelPool.GetPooledObject();
                    currencyObject.transform.SetParent(parentTrasnform);
                    currencyObject.transform.ResetLocal();
                    currencyObject.transform.SetAsLastSibling();
                    currencyObject.SetActive(true);

                    CurrencyUI currencyUI = currencyObject.GetComponent<CurrencyUI>();
                    currencyUI.Initialise(currencies[i]);
                    currencyUI.Show();

                    activePanelsUI.Add(currencies[i].CurrencyType, currencyUI);
                }
            }
        }

        public void ActivateAllExistingCurrencies()
        {
            Currency[] activeCurrencies = CurrenciesController.Currencies;
            for (int i = 0; i < activeCurrencies.Length; i++)
            {
                if (activeCurrencies[i].Amount > 0)
                    ActivateCurrency(activeCurrencies[i].CurrencyType);
            }
        }

        public void RedrawCurrency(Currency.Type type, int amount)
        {
            if (activePanelsUI.ContainsKey(type))
            {
                activePanelsUI[type].SetAmount(amount);
            }
            else
            {
                ActivateCurrency(type);
            }
        }

        // doNotHide will prevent currency from auto hide - call DisableCurrency to hide it manually
        public void ActivateCurrency(Currency.Type type, bool doNotHide = false)
        {
            // Check if panel is disabled
            if (!activePanelsUI.ContainsKey(type))
            {
                // Get object from pool
                GameObject currencyObject = panelPool.GetPooledObject();
                currencyObject.transform.SetParent(parentTrasnform);
                currencyObject.transform.ResetLocal();
                currencyObject.transform.SetAsLastSibling();
                currencyObject.SetActive(true);

                // Get currency from database
                Currency currency = CurrenciesController.GetCurrency(type);

                // Get UI panel component
                CurrencyUI currencyUI = currencyObject.GetComponent<CurrencyUI>();
                currencyUI.Initialise(currency);
                currencyUI.Show();

                // Check if panel require disable
                if (!currency.DisplayAlways && !doNotHide)
                {
                    currencyUI.DisableAfter(DISALBE_PANEL_IN_SECONDS, delegate
                    {
                        if (activePanelsUI.ContainsKey(type))
                            activePanelsUI.Remove(type);
                    });
                }

                activePanelsUI.Add(type, currencyUI);
            }
            // Rewrite panel state
            else
            {
                CurrencyUI currencyUI = activePanelsUI[type];

                // Check if panel require disable reset
                if (!currencyUI.Currency.DisplayAlways)
                {
                    currencyUI.ShowImmediately();
                    currencyUI.ResetDisable();

                    if (doNotHide)
                        currencyUI.KillDisable();
                }

                // Redraw
                currencyUI.Redraw();
            }
        }

        public void DisableCurrency(Currency.Type type)
        {
            // check if panel is active state
            if (activePanelsUI.ContainsKey(type) && !CurrenciesController.GetCurrency(type).DisplayAlways)
            {
                activePanelsUI[type].Hide();
                activePanelsUI.Remove(type);

            }
        }
    }
}