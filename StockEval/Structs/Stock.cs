using Microsoft.ML.Data;
public class StockModel
{
    [LoadColumn(0)]
    public DateTime Date;
    [LoadColumn(1)]
    public float Open;
    [LoadColumn(4)]
    public float Close;
}