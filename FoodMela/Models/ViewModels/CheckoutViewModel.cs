using System.Collections.Generic;

namespace FoodMela.Models
{
    public class CheckoutViewModel
    {
        public IEnumerable<CartItem> CartItems { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPhone { get; set; }
    }
}
