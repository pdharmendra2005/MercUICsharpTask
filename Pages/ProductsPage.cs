using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject2.Helper;

namespace TestProject2.Pages
{
    public class ProductsPage : BasePage
    {
        #region Elements
        private By InventoryPrices => By.XPath("//div[@data-test='inventory-item-price']");
        private By InventoryItemNameUsingPrice(string price) => By.XPath($"//div[@data-test='inventory-item-price' and contains(., '{price}')]/ancestor::div[@data-test='inventory-item-description']//div[@data-test='inventory-item-name']");
        private By AddToCartButton => By.Id("add-to-cart");
        private By CartIcon => By.XPath("//span[@data-test='shopping-cart-badge']");
        
        #endregion

        public ProductsPage(IWebDriver driver) : base(driver) 
        {
        }

        #region Functions

        public decimal GetHighestInventoryPrice()
        {
            try
            {
                // Find all price elements
                var priceElements = FindElements(InventoryPrices);

                if (!priceElements.Any())
                {
                    throw new NoSuchElementException("No inventory prices found on the page");
                }

                // Extract text and parse prices
                // The element.Text will get all text content, including the $ and amount
                var prices = priceElements
                    .Select(element => element.Text.Replace("$", "").Trim())
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Select(text => decimal.Parse(text))
                    .ToList();

                return prices.Max();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get highest inventory price: {ex.Message}", ex);
            }
        }


        public void ClickHighestPricedItem()
        {
            // Get the highest price
            decimal highestPrice = GetHighestInventoryPrice();

            // Format the price as a string
            string priceString = highestPrice.ToString("0.00");

            ClickElement(InventoryItemNameUsingPrice(priceString));
        }

        public void ClickOnAddToCartButton()
        {
            ClickElement(AddToCartButton);
        }

        #endregion

    }
}
