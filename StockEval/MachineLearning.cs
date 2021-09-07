using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

/// <summary>
/// Docs: https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/time-series-demand-forecasting
/// </summary>
public class MachineLearning
{
    public static IDataView ReadDataFromFile(MLContext context, string dataPath)
    {
        var view = context.Data.LoadFromTextFile<StockModel>(dataPath, hasHeader: true, separatorChar: ',');
        var enumerable = context.Data.CreateEnumerable<StockModel>(view, reuseRowObject: false);
        var validValues = context.Data.LoadFromEnumerable(enumerable.Where(s => !float.IsNaN(s.Open) && !float.IsNaN(s.Close)));
        return validValues;
    }
    public static SsaForecastingTransformer Train(MLContext context, IDataView view)
    {
        var pipeline = context.Forecasting.ForecastBySsa(outputColumnName: "PredictedPrices",
            inputColumnName: "Close",
            windowSize: 7,
            seriesLength: 30,
            trainSize: 365,
            horizon: 7,
            confidenceLevel: 0.95f);

        var model = pipeline.Fit(view);
        
        return model;
    }
}