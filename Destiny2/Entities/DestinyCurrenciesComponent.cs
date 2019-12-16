using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Destiny2.Entities
{
	public class DestinyCurrenciesComponent
	{
		[JsonProperty(PropertyName = "itemQuantities")]
		public IDictionary<uint, int> ItemQuantities { get; set; }
	}
}
