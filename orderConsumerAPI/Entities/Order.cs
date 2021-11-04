using System.Collections.Generic;

namespace orderProducerAPI.Entities
{
    public class Order
    {
        public int numero { get; set; }

        //public string ItemName { get; set; }

        public string cnpj { get; set; }

        public bool status { get; set; }

        public List<Item> itens { get; set; }

       
    }
}
