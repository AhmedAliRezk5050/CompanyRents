namespace Infrastructure.Utility;

public class Permissions
{
    public static List<string> GeneratePermissions(string module)
    {
        return new List<string>
        {
            $"Permission.{module}.View",
            $"Permission.{module}.Add",
            $"Permission.{module}.Edit",
            $"Permission.{module}.Delete",
        };
    }
    
    public static List<string> GetAllPermissions()
    {
        var allPermissions = new List<string>();

        foreach (var item in Enum.GetValues(typeof(Modules)))
        {
            allPermissions.AddRange(GeneratePermissions(item.ToString()));
        }

        return allPermissions;
    }
    
    public static class Lessors
    {
        public const string View = "Permission.Lessors.View";
        public const string Add = "Permission.Lessors.Add";
        public const string Edit = "Permission.Lessors.Edit";
        public const string Delete = "Permission.Lessors.Delete";
    }
    
    public static class Renewals
    {
        public const string View = "Permission.Renewals.View";
        public const string Add = "Permission.Renewals.Add";
        public const string Edit = "Permission.Renewals.Edit";
        public const string Delete = "Permission.Renewals.Delete";
    }
    
    public static class Invoices
    {
        public const string View = "Permission.Invoices.View";
        public const string Add = "Permission.Invoices.Add";
        public const string Edit = "Permission.Invoices.Edit";
        public const string Delete = "Permission.Invoices.Delete";
    }
    
    public static class Payments
    {
        public const string View = "Permission.Payments.View";
        public const string Add = "Permission.Payments.Add";
        public const string Edit = "Permission.Payments.Edit";
        public const string Delete = "Permission.Payments.Delete";
    }
    
    public static class Users
    {
        public const string View = "Permission.Users.View";
        public const string Add = "Permission.Users.Add";
        public const string Edit = "Permission.Users.Edit";
        public const string Delete = "Permission.Users.Delete";
    }
    
    public static class Claims
    {
        public const string View = "Permission.Claims.View";
        public const string Add = "Permission.Claims.Add";
        public const string Edit = "Permission.Claims.Edit";
        public const string Delete = "Permission.Claims.Delete";
    }
}