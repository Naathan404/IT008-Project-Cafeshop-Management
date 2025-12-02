using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Models
{
	public partial class Order
	{
		public string DisplayID
		{
			get
			{
				return $"HD{OrderDate:yyMMdd}-{OrderId:D5}";
			}
		}
	}
}
