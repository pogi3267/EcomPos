namespace ApplicationWeb.HelperAndConstant
{
    public class RequiredPolicyAttribute
    {
        public string? PolicyName { get; set; }
        public string? ClaimName { get; set; }
        public string? Value { get; set; }
    }

    public static class RequiredPolicy
    {
        public static List<RequiredPolicyAttribute> GetRequiredPolicy()
        {
            return new List<RequiredPolicyAttribute>
            {
                #region Color

                new RequiredPolicyAttribute
                {
                    PolicyName = "ColorCreatePolicy" ,
                    ClaimName = "Color.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "ColorEditPolicy" ,
                    ClaimName = "Color.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "ColorDeletePolicy" ,
                    ClaimName = "Color.Delete",
                    Value = "true",
                },

                #endregion Color

                #region category

                new RequiredPolicyAttribute
                {
                    PolicyName = "CategoryCreatePolicy" ,
                    ClaimName = "Category.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CategoryEditPolicy" ,
                    ClaimName = "Category.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CategoryDeletePolicy" ,
                    ClaimName = "Category.Delete",
                    Value = "true",
                },

                #endregion category

                #region Customer & Supplier
                 new RequiredPolicyAttribute
                {
                    PolicyName = "OperationalUserSetupCreatePolicy" ,
                    ClaimName = "OperationalUserSetup.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "OperationalUserSetupEditPolicy" ,
                    ClaimName = "OperationalUserSetup.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "OperationalUserSetupDeletePolicy" ,
                    ClaimName = "OperationalUserSetup.Delete",
                    Value = "true",
                },
                #endregion

                #region Brand

                new RequiredPolicyAttribute
                {
                    PolicyName = "BrandCreatePolicy" ,
                    ClaimName = "Brand.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "BrandEditPolicy" ,
                    ClaimName = "Brand.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "BrandDeletePolicy" ,
                    ClaimName = "Brand.Delete",
                    Value = "true",
                },

                #endregion Brand
                #region Collection

                new RequiredPolicyAttribute
                {
                    PolicyName = "CollectionCreatePolicy" ,
                    ClaimName = "Collection.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CollectionEditPolicy" ,
                    ClaimName = "Collection.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CollectionDeletePolicy" ,
                    ClaimName = "Collection.Delete",
                    Value = "true",
                },

                #endregion Collection

                #region Attribute

                new RequiredPolicyAttribute
                {
                    PolicyName = "AttributeCreatePolicy" ,
                    ClaimName = "Attribute.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "AttributeEditPolicy" ,
                    ClaimName = "Attribute.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "AttributeDeletePolicy" ,
                    ClaimName = "Attribute.Delete",
                    Value = "true",
                },

                #endregion Attribute

                 #region Item
                new RequiredPolicyAttribute
                {
                    PolicyName = "ItemCreatePolicy" ,
                    ClaimName = "ItemSetup.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "ItemEditPolicy" ,
                    ClaimName = "ItemSetup.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "ItemDeletePolicy" ,
                    ClaimName = "ItemSetup.Delete",
                    Value = "true",
                },
                #endregion

                #region Unit

                new RequiredPolicyAttribute
                {
                    PolicyName = "UnitCreatePolicy" ,
                    ClaimName = "Unit.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "UnitEditPolicy" ,
                    ClaimName = "Unit.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "UnitDeletePolicy" ,
                    ClaimName = "Unit.Delete",
                    Value = "true",
                },

                #endregion Unit

                #region General Setting

                new RequiredPolicyAttribute
                {
                    PolicyName = "GeneralSettingsCreatePolicy" ,
                    ClaimName = "GeneralSetting.Create",
                    Value = "true",
                },

                #endregion General Setting

                #region Currency

                new RequiredPolicyAttribute
                {
                    PolicyName = "CurrencyCreatePolicy" ,
                    ClaimName = "Currency.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CurrencyEditPolicy" ,
                    ClaimName = "Currency.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CurrencyDeletePolicy" ,
                    ClaimName = "Currency.Delete",
                    Value = "true",
                },

                #endregion Currency

                #region VatAndTax

                new RequiredPolicyAttribute
                {
                    PolicyName = "VatAndTaxCreatePolicy" ,
                    ClaimName = "VatAndTax.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "VatAndTaxEditPolicy" ,
                    ClaimName = "VatAndTax.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "VatAndTaxDeletePolicy" ,
                    ClaimName = "VatAndTax.Delete",
                    Value = "true",
                },

                #endregion VatAndTax

                #region Pickup Point
                new RequiredPolicyAttribute
                {
                    PolicyName = "PickupPointCreatePolicy" ,
                    ClaimName = "PickupPoint.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "PickupPointEditPolicy" ,
                    ClaimName = "PickupPoint.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "PickupPointDeletePolicy" ,
                    ClaimName = "PickupPoint.Delete",
                    Value = "true",
                },
                #endregion

                #region Payment Methods
                new RequiredPolicyAttribute
                {
                    PolicyName = "PaymentMethodsCreatePolicy" ,
                    ClaimName = "PaymentMethods.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "PaymentMethodsEditPolicy" ,
                    ClaimName = "PaymentMethods.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "PaymentMethodsDeletePolicy" ,
                    ClaimName = "PaymentMethods.Delete",
                    Value = "true",
                },
                #endregion

                #region Shipping Country
                new RequiredPolicyAttribute
                {
                    PolicyName = "CountryCreatePolicy" ,
                    ClaimName = "ShippingCountris.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CountryEditPolicy" ,
                    ClaimName = "ShippingCountris.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CountryDeletePolicy" ,
                    ClaimName = "ShippingCountris.Delete",
                    Value = "true",
                },
                #endregion

                #region Shipping State
                new RequiredPolicyAttribute
                {
                    PolicyName = "StateCreatePolicy" ,
                    ClaimName = "ShippingStates.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "StateEditPolicy" ,
                    ClaimName = "ShippingStates.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "StateDeletePolicy" ,
                    ClaimName = "ShippingStates.Delete",
                    Value = "true",
                },
                #endregion

                #region Shipping City
                new RequiredPolicyAttribute
                {
                    PolicyName = "CityCreatePolicy" ,
                    ClaimName = "ShippingCities.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CityEditPolicy" ,
                    ClaimName = "ShippingCities.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CityDeletePolicy" ,
                    ClaimName = "ShippingCities.Delete",
                    Value = "true",
                },
                #endregion

                #region Staff
                new RequiredPolicyAttribute
                {
                    PolicyName = "StaffCreatePolicy" ,
                    ClaimName = "Staff.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "StaffEditPolicy" ,
                    ClaimName = "Staff.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "StaffDeletePolicy" ,
                    ClaimName = "Staff.Delete",
                    Value = "true",
                },
                #endregion

                #region Reviews

                new RequiredPolicyAttribute
                {
                    PolicyName = "ReviewsCreatePolicy",
                    ClaimName = "Reviews.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "ReviewsEditPolicy",
                    ClaimName = "Reviews.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "ReviewsDeletePolicy",
                    ClaimName = "Reviews.Delete",
                    Value = "true",
                },

                #endregion Reviews

                #region Adjustment
                new RequiredPolicyAttribute
                {
                    PolicyName = "AdjustmentCreatePolicy" ,
                    ClaimName = "AdjustmentSetup.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "AdjustmentEditPolicy" ,
                    ClaimName = "AdjustmentSetup.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "AdjustmentDeletePolicy" ,
                    ClaimName = "AdjustmentSetup.Delete",
                    Value = "true",
                },
                #endregion

                #region Payment
                new RequiredPolicyAttribute
                {
                    PolicyName = "PaymentCreatePolicy" ,
                    ClaimName = "UserPayment.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "PaymentEditPolicy" ,
                    ClaimName = "UserPayment.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "PaymentDeletePolicy" ,
                    ClaimName = "UserPayment.Delete",
                    Value = "true",
                },
                #endregion

                 #region AccountLedger
                new RequiredPolicyAttribute
                {
                    PolicyName = "ChartOfAccountCreatePolicy" ,
                    ClaimName = "ChartOfAccount.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "ChartOfAccountEditPolicy" ,
                    ClaimName = "ChartofAccount.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "ChartOfAccountDeletePolicy" ,
                    ClaimName = "ChartofAccount.Delete",
                    Value = "true",
                },
                #endregion

                #region Cost
                new RequiredPolicyAttribute
                {
                    PolicyName = "CostCreatePolicy" ,
                    ClaimName = "CostSetup.Create",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CostEditPolicy" ,
                    ClaimName = "CostSetup.Edit",
                    Value = "true",
                },
                new RequiredPolicyAttribute
                {
                    PolicyName = "CostDeletePolicy" ,
                    ClaimName = "CostSetup.Delete",
                    Value = "true",
                },
                #endregion


            };
        }
    }
}