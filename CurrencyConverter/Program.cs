using Microsoft.Extensions.DependencyInjection;
using Services;

internal class Program
{
    private static void Main(string[] args)
    {
        static void logConvertOperation(string from, string to, double amount, double convrtedAmount) => Console.WriteLine($"Converting {amount} From {from} To {to} Return {convrtedAmount}");

        var serviceProvider = new ServiceCollection().AddSingleton<ICurrencyConverter, CurrencyConverter>().BuildServiceProvider();
        var converter = serviceProvider.GetRequiredService<ICurrencyConverter>();

        IEnumerable<Tuple<string, string, double>> convertionRates = [new("USD", "CAD", 1.34), new("CAD", "GBP", 0.58), new("USD", "EUR", 0.86), new("IRR", "USD", 1.5), new("TR", "MK", 1.5)];

        converter.ClearConfiguration();
        converter.UpdateConfiguration(convertionRates);

        try
        {
            double firstConvert = converter.Convert("USD", "CAD", 1500);
            logConvertOperation("USD", "CAD", 1500, firstConvert);

            double secondConvert = converter.Convert("GBP", "EUR", 1500);
            logConvertOperation("GBP", "EUR", 1500, secondConvert);

            double thirdConvert = converter.Convert("IRR", "EUR", 1500);
            logConvertOperation("IRR", "EUR", 1500, thirdConvert);

            double invalidConvert = converter.Convert("MK", "EUR", 1500);
            logConvertOperation("MK", "EUR", 1500, invalidConvert);
        }
        catch (Exception e)
        {

            Console.WriteLine(e.Message);
        }
        Console.ReadLine();
    }
}