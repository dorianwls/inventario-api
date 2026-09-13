namespace Inventario.Api.Auth;

public static class ApplicationRoles
{
    public const string Administrator = "Administrator";
    public const string Supervisor = "Supervisor";
    public const string Seller = "Seller";
    public const string Warehouse = "Warehouse";
    public const string Accountant = "Accountant";

    public static readonly string[] All =
    [
        Administrator,
        Supervisor,
        Seller,
        Warehouse,
        Accountant,
    ];
}
