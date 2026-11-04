namespace FCG.Catalog.Infrastructure.Data;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "FCG_Catalog";
}
