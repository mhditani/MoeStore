using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Services
{
    public class OrderHelper
    {
        public static decimal ShippingFee { get; } = 5;

        public static Dictionary<string, string> PaymentMethods { get; } = new Dictionary<string, string>()
        {
            {"Cash", "Cash on delivery" },
            {"Paypal", "Paypal" },
            {"Credit Card", "Credit Card" }
        };

        public static List<string> PaymentStatus { get; } = new List<string>()
        {
            "Pending", "Accepted", "Canceled"
        };

        public static List<string> OrderStatus { get; } = new List<string>()
        {
            "Created", "Accepted", "Canceled", "Shipped", "Delivered", "Returned"
        };

        /*
         * Recieves a string of products identifiers, separated by '-'
         * Example 9-9-7-9-6
         * 
         * Returns a list of pairs (dictionary)
         *     -the pair name is the product ID
         *     -the pair value is the product quantity
         *Example:{
         *   9: 3,
         *   7: 1,
         *   6: 1
         *}
         */
        public static Dictionary<int, int> GetProductDictionary(string productIdentifiers)
        {
            var productDictionary = new Dictionary<int, int>();

            if (productIdentifiers.Length > 0)
            {
                string[] productIdArray = productIdentifiers.Split('.');
                foreach (string productId in productIdArray)
                {
                    try
                    {
                        int id = int.Parse(productId);
                        if (productDictionary.ContainsKey(id))
                        {
                            productDictionary[id] += 1;
                        }else
                        {
                            productDictionary.Add(id, 1);
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }

            return productDictionary;
        }
    }
}
