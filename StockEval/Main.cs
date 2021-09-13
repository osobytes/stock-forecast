
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

//1) Pedir Input de Stock ID.
var stockId = args[0];
if (string.IsNullOrEmpty(stockId))
{
    Console.WriteLine("Id de stock invalido.");
    return;
}

//2) Url de donde obtendremos los datos historicos del stock.
const string yahooEndpoint = "https://query1.finance.yahoo.com";
//3) Obtener periodo inicial (Hace 12 Meses) y periodo actual (hoy)
var fromPeriod = DateTimeOffset.UtcNow.AddMonths(-12).ToUnixTimeSeconds();
var toPeriod = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//4) Crear URL para obtener el Stock.
var url = $"{yahooEndpoint}/v7/finance/download/{stockId}?period1={fromPeriod}&period2={toPeriod}&interval=1d&events=history&includeAdjustedClose=true";
//5) Descargar datos del URL.
using var client = new HttpClient();
var response = await client.GetAsync(url);
//6) Guardar datos en un archivo en disco duro.
var fileResult = await response.Content.ReadAsStreamAsync();
var outputPath = Path.GetTempPath();
var dataInputPath = Path.Combine(outputPath, $"{stockId}_history_{fromPeriod}_to_{toPeriod}.csv");
using (var outputFileStream = new FileStream(dataInputPath, FileMode.Create))
{
    fileResult.CopyTo(outputFileStream);
}
//7) Inicializar contexto de Machine Learning
var context = new MLContext(seed: 0);
//8) Convertir el archivo CSV descargado en un elemento en memoria.
var view = context.Data.LoadFromTextFile<StockModel>(dataInputPath, hasHeader: true, separatorChar: ',');
var enumerable = context.Data.CreateEnumerable<StockModel>(view, reuseRowObject: false);
var data = context.Data.LoadFromEnumerable(enumerable.Where(s => !float.IsNaN(s.Open) && !float.IsNaN(s.Close)));
//9) Entrenar Modelo de Machine Learning
// Docs: https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/time-series-demand-forecasting
var pipeline = context.Forecasting.ForecastBySsa(outputColumnName: "PredictedPrices",
    inputColumnName: "Close",
    windowSize: 7,
    seriesLength: 30,
    trainSize: 365,
    horizon: 7,
    confidenceLevel: 0.95f);
var model = pipeline.Fit(data);
//10) Hacer un forecast con el nuevo modelo y ejecutar prediccion.
var forecastEngine = model.CreateTimeSeriesEngine<StockModel, ForecastOutput>(context);
var forecastResult = forecastEngine.Predict();
//11) Imprimir Resultados.
for(var i = 0; i < forecastResult.PredictedPrices.Length; i++)
{
    var predictedPrice = forecastResult.PredictedPrices[i];
    var date = DateTime.UtcNow.AddDays(i).Date;
    Console.WriteLine($"Precio estimado en {date:dd-MMM-yyyy}: {predictedPrice}");
}