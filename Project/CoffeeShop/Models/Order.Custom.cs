namespace CoffeeShop.Models;

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
