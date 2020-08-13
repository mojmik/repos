using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PrikladEshop
{
   
        /// <summary>
        /// Tato třída reprezentuje platební bránu nějakého většího systému třetí strany. V reálu by zde tedy bylo
        /// ještě spoustu dalších tříd a metod.
        /// </summary>
        class Gateway
        {
            /// <summary>
            /// Metoda simuluje zpracování objednávky, zde pouze vypsáním do konzole. Důležité je, že díky dodržení
            /// rozhraní od vývojářů platební brány umí brána pracovat s naší objednávkou, i když naši třídu nikdy
            /// neviděli.
            /// </summary>
            /// <param name="order">Objednávka implementující rozhraní IOrder</param>
            public void ProcessOrder(IOrder order)
            {
                Console.WriteLine("Order no. {0}", order.Number);
                Console.WriteLine("=============");
                Console.WriteLine("Name:    {0} {1}", order.FirstName, order.LastName);
                Console.WriteLine("Address: {0} {1}/{2}", order.Street, order.HouseNumber, order.RegistryNumber);
                Console.WriteLine("         {0} {1}", order.City, order.Zip);
                Console.WriteLine("         {0}", order.Country);
                Console.WriteLine();
                for (int i = 0; i < order.Products.Length; i++)
                {
                    Console.WriteLine("{0} {1}pcs per {2},-", order.Products[i], order.Quantities[i], order.Prices[i]);
                }
                Console.WriteLine();
                Console.WriteLine("Total price: {0},-", order.Prices.Sum());
            }

        }

}
