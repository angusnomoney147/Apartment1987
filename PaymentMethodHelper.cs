namespace ApartmentManagementSystem
{
    public static class PaymentMethodHelper
    {
        public static string GetMethodName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "Cash",
                PaymentMethod.BankTransfer => "Bank Transfer",
                PaymentMethod.CreditCard => "Credit Card",
                PaymentMethod.Check => "Check",
                _ => "Unknown"
            };
        }
    }
}