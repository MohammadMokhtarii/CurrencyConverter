namespace Services;

public interface ICurrencyConverter
{
    /// <summary>
    /// Clears any prior configuration.
    /// </summary>
    void ClearConfiguration();
    /// <summary>
    /// Updates the configuration. Rates are inserted or replaced internally.
    /// </summary>
    void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates);

    /// <summary>
    /// Converts the specified amount to the desired currency.
    /// </summary>
    double Convert(string fromCurrency, string toCurrency, double amount);
}



public class CurrencyConverter : ICurrencyConverter
{
    private readonly IDictionary<string, IDictionary<string, double>> _rateGraph;
    private static readonly object _grapLock = new();

    public CurrencyConverter() => _rateGraph = new Dictionary<string, IDictionary<string, double>>();
    public void ClearConfiguration()
    {
        lock (_grapLock)
            _rateGraph.Clear();
    }
    public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
    {
        lock (_grapLock)
        {
            foreach (var conversionRate in conversionRates)
            {
                string from = conversionRate.Item1;
                string to = conversionRate.Item2;
                double rate = conversionRate.Item3;

                if (_rateGraph.TryGetValue(from, out var fromGraph))
                    fromGraph[to] = rate;
                else
                    _rateGraph[from] = new Dictionary<string, double>() { { to, rate } };


                if (_rateGraph.TryGetValue(to, out var toGraph))
                    toGraph[from] = 1 / rate;
                else
                    _rateGraph[to] = new Dictionary<string, double>() { { from, 1 / rate } };
            }
        }
    }
    public double Convert(string fromCurrency, string toCurrency, double amount)
    {

        HashSet<string> visitedNodes = [];
        Queue<Tuple<string, double>> nodeQueue = new();
        nodeQueue.Enqueue(new Tuple<string, double>(fromCurrency, amount));
        while (nodeQueue.Count > 0)
        {
            var (currentCurrency, currentAmount) = nodeQueue.Dequeue();
            visitedNodes.Add(currentCurrency);

            if (_rateGraph.TryGetValue((currentCurrency), out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    string neighborCurrency = neighbor.Key;
                    double neighborRate = neighbor.Value;
                    if (neighborCurrency == toCurrency)
                        return currentAmount * neighborRate;

                    if (!visitedNodes.TryGetValue(neighborCurrency, out _))
                        nodeQueue.Enqueue(new Tuple<string, double>(neighborCurrency, currentAmount * neighborRate));
                }
            }
        }
        // maybe exception
        // maybe use result pattern to return behavior => i Prefer this
        throw new InvalidCastException($"We Are Not Supporting The Requested Conversion:{fromCurrency} to {toCurrency}");
    }

    
}
