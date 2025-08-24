namespace ApartmentManagementSystem
{
    public static class PaymentMethodHelper
    {
        public static string GetMethodName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "Cash",
                PaymentMethod.Check => "Check",
                PaymentMethod.CreditCard => "Credit Card",
                PaymentMethod.BankTransfer => "Bank Transfer",
                PaymentMethod.Other => "Other",
                _ => method.ToString()
            };
        }
    }
}