/// <summary>
/// Herramienta utilizada para obtener historicos de Yahoo Finance.
/// </summary>
public static class YahooFinance
{
    /// <summary>
    /// Url para obtener datos de Yahoo Finance.
    /// Se debe rellenar con periodos y el nombre del stock.
    /// </summary>
    const string DownloadUri = "https://query1.finance.yahoo.com/v7/finance/download/{0}?period1={1}&period2={2}&interval=1d&events=history&includeAdjustedClose=true";

    /// <summary>
    /// Este metodo se encarga de descargar el historico de los stocks de un simbolo otorgado de los ultimos n meses otorgados.
    /// </summary>
    /// <param name="symbol">El simbolo para descargar el historico.</param>
    /// <param name="months"></param>
    /// <returns>Regresa el nombre del archivo .csv donde se han descargado los historicos.</returns>
    public static async Task<string> GetSymbolHistory(string symbol, int months = 12)
    {
        // Obtengo periodo de datos.
        // periodos: Desde y Hasta.
        var fromPeriod = DateTimeOffset.UtcNow.AddMonths(-months).ToUnixTimeSeconds();
        var toPeriod = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Obtener URL de Yahoo Finance para obtener los datos requeridos.
        var url = string.Format(DownloadUri, symbol, fromPeriod, toPeriod);

        // usar http client para obtener datos de un url.
        using var client = new HttpClient();
        var response = await client.GetAsync(url);

        // Guardar archivo en disco duro.
        var fileResult = await response.Content.ReadAsStreamAsync();
        var outputPath = Path.GetTempPath();
        var filePath = Path.Combine(outputPath, $"{symbol}_history_{fromPeriod}_to_{toPeriod}.csv");
        using var outputFileStream = new FileStream(filePath, FileMode.Create);
        fileResult.CopyTo(outputFileStream);
        return filePath;
    }
}