using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject2.Pages;

namespace TestProject2.StepDefinitions
{
    [Binding]
    public class ProductsStepDefination : ProductsPage
    {
        public ProductsStepDefination(IWebDriver driver) : base(driver) { }

        [When("I select the highest priced item on the page")]
        public void WhenISelectTheHighestPricedItemOnThePage()
        {
            ClickHighestPricedItem();
        }

        [Then("I should be able to add the item to cart")]
        public void ThenIShouldBeAbleToAddTheItemToCart()
        {
            ClickOnAddToCartButton();
        }

    }
}
