namespace AeonRegistryAPI.Data
{
    public static class DataUtility
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DbConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            return connectionString!;
        }
    }
}
