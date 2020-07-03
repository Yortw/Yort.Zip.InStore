using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Represents information about an item purchased on a Zip order, used to show the end-consumer details of their purchase in the Zip consumer portal.
	/// </summary>
	public class ZipOrderItem
	{
		/// <summary>
		/// A description of the item.
		/// </summary>
		public string Description { get; set; } = null!;

		/// <summary>
		/// A short name of the item.
		/// </summary>
		public string Name { get; set; } = null!;
		/// <summary>
		/// The unique sku code of the item.
		/// </summary>
		public string Sku { get; set; } = null!;
		/// <summary>
		/// The quantity of this item purchased.
		/// </summary>
		public int Quantity { get; set; }
		/// <summary>
		/// The price the item was purchased at.
		/// </summary>
		public decimal Price { get; set; }
	}
}
