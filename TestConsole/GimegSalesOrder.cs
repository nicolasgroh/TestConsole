using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    public class GimegSalesOrder
    {
        [Name("Purchase order")]
        public string PurchaseOrder { get; set; }

        [Name("Purchase journal")]
        public string PurchaseJournal { get; set; }

        [Name("Delivery name")]
        public string DeliveryName { get; set; }

        [Name("Delivery name IG-shop")]
        public string DeliveryNameIGShop { get; set; }

        [Name("Delivery street")]
        public string DeliveryStreet { get; set; }

        [Name("Delivery City")]
        public string DeliveryCity { get; set; }

        [Name("Delivery postal code")]
        public string DeliveryPostalCode { get; set; }

        [Name("Delivery country")]
        public string DeliveryCountry { get; set; }

        [Name("Customer name")]
        public string CustomerName { get; set; }

        [Name("Customer street")]
        public string CustomerStreet { get; set; }

        [Name("Customer city")]
        public string CustomerCity { get; set; }

        [Name("Customer postal code")]
        public string CustomerPostalCode { get; set; }

        [Name("Customer country")]
        public string CustomerCountry { get; set; }

        [Name("Customer reference")]
        public string CustomerReference { get; set; }

        [Name("Vendor reference")]
        public string VendorReference { get; set; }

        [Name("Vendor reference GTIN")]
        public string VendorReferenceGTIN { get; set; }

        [Name("Sales origin")]
        public string SalesOrigin { get; set; }

        [Name("Email")]
        public string EMail { get; set; }

        [Name("Phone")]
        public string Phone { get; set; }

        [Name("Phone mobile")]
        public string PhoneMobile { get; set; }

        [Name("Order date")]
        [Format("yyyy/MM/dd")]
        public DateTime? OrderDate { get; set; }

        [Name("Item")]
        public string Item { get; set; }

        [Name("Quantity")]
        public decimal Quantity { get; set; }

        [Name("External item")]
        public string ExternalItem { get; set; }

        [Name("Delivery date")]
        [Format("yyyy/MM/dd")]
        public DateTime? DeliveryDate { get; set; }

        [Name("Purchase price")]
        public decimal PurchasePrice { get; set; }

        [Name("Line amount")]
        public decimal LineAmount { get; set; }

        [Name("Line amount tax")]
        public decimal LineAmountTax { get; set; }

        [Name("Currency code")]
        public string CurrencyCode { get; set; }

        [Name("Amount")]
        public decimal Amount { get; set; }

        //[Name("Original Customer Reference")]
        //public string OriginalCustomerReference { get; set; }

        public void CopyTo(GimegSalesOrder salesOrder)
        {
            salesOrder.PurchaseOrder = PurchaseOrder;
            salesOrder.PurchaseJournal = PurchaseJournal;
            salesOrder.DeliveryName = DeliveryName;
            salesOrder.DeliveryNameIGShop = DeliveryNameIGShop;
            salesOrder.DeliveryStreet = DeliveryStreet;
            salesOrder.DeliveryCity = DeliveryCity;
            salesOrder.DeliveryPostalCode = DeliveryPostalCode;
            salesOrder.DeliveryCountry = DeliveryCountry;
            salesOrder.CustomerName = CustomerName;
            salesOrder.CustomerStreet = CustomerStreet;
            salesOrder.CustomerCity = CustomerCity;
            salesOrder.CustomerPostalCode = CustomerPostalCode;
            salesOrder.CustomerCountry = CustomerCountry;
            salesOrder.CustomerReference = CustomerReference;
            salesOrder.VendorReference = VendorReference;
            salesOrder.VendorReferenceGTIN = VendorReferenceGTIN;
            salesOrder.SalesOrigin = SalesOrigin;
            salesOrder.EMail = EMail;
            salesOrder.Phone = Phone;
            salesOrder.PhoneMobile = PhoneMobile;
            salesOrder.OrderDate = OrderDate;
            salesOrder.Item = Item;
            salesOrder.Quantity = Quantity;
            salesOrder.ExternalItem = ExternalItem;
            salesOrder.DeliveryDate = DeliveryDate;
            salesOrder.PurchasePrice = PurchasePrice;
            salesOrder.LineAmount = LineAmount;
            salesOrder.LineAmountTax = LineAmountTax;
            salesOrder.CurrencyCode = CurrencyCode;
            salesOrder.Amount = Amount;
        }
    }
}
