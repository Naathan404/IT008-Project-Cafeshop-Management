namespace CoffeeShop.Models;


public partial class Order
{
    // Scaffold-DbContext "Server=LAPTOP-UEB0IP6O;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context CoffeeShopContext -Force
    public string DisplayID
	{
		get
		{
			return $"HD{OrderDate:yyMMdd}-{OrderId:D5}";
		}
	}

}
