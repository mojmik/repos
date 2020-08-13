using System;
using System.Collections.Generic;
using System.Text;

namespace PrikladEshop
{
    interface IOrder
    {
        int Number { get; }
        /// <summary>
        /// Customer first name
        /// </summary>
        string FirstName { get; }
        /// <summary>
        /// Customer last name
        /// </summary>
        string LastName { get; }
        /// <summary>
        /// Street
        /// </summary>
        string Street { get; }
        /// <summary>
        /// House number
        /// </summary>
        int HouseNumber { get; }
        /// <summary>
        /// Registry number
        /// </summary>
        int RegistryNumber { get; }
        /// <summary>
        /// City
        /// </summary>
        string City { get; }
        /// <summary>
        /// Zip code
        /// </summary>
        string Zip { get; }
        /// <summary>
        /// Country
        /// </summary>
        string Country { get; }
        /// <summary>
        /// Product names
        /// </summary>
        string[] Products { get; }
        /// <summary>
        /// Product quantities
        /// </summary>
        int[] Quantities { get; }
        /// <summary>
        /// Product prices
        /// </summary>
        double[] Prices { get; }
    }
}
