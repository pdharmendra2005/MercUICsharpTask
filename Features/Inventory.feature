@Inventory
Feature: Inventory

Purpose of this feature is to login and select highest price item from
the inventory page and then click on the add to cart button

Scenario: Add the highest prices item to cart
	Given I am on the login page
	When I login with username "standard_user" and password "secret_sauce"
	And I select the highest priced item on the page
	Then I should be able to add the item to cart
