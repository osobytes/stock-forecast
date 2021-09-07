using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

// Pedir Input (Stock ID)
Console.WriteLine("Inserte el id del stock para predecir su presio en los siguientes 7 dias.");
var symbol = Console.ReadLine();

// Descargamos los datos del simbolo de Stock otorgado.
var dataInput = await YahooFinance.GetSymbolHistory(symbol);
// Inicializamos el contexto de machine learning.
var context = new MLContext(seed: 0);
// Convertimos el archivo csv a una lista de elementos en memoria.
var data = MachineLearning.ReadDataFromFile(context, dataInput);
// Entrenamos un modelo en base a esos datos
var model = MachineLearning.Train(context, data);
// Hacemos un forecast en base a los datos entrenados.
var forecastEngine = model.CreateTimeSeriesEngine<StockModel, ForecastOutput>(context);
// Ejecutamos prediccion.
var forecastResult = forecastEngine.Predict();

// Iteramos entre los resultados de Forecast Result para imprimir los precios estimados.
for(var i = 0; i <  forecastResult.PredictedPrices.Length; i++)
{
    var predictedPrice = forecastResult.PredictedPrices[i];
    var date = DateTime.UtcNow.AddDays(i).Date;
    Console.WriteLine();
    Console.WriteLine($"Precio estimado en {date:dd-MMM-yyyy}: {predictedPrice}");
}