using Npgsql;

var connStr = "Host=192.168.3.22;Port=5432;Database=kongkong_bytebite;Username=konghao;Password=hitek.123";
using var conn = new NpgsqlConnection(connStr);
conn.Open();

System.Console.WriteLine("Current migration history:");
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "SELECT \"MigrationId\", \"ProductVersion\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\"";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        System.Console.WriteLine($"   {reader.GetString(0)} | {reader.GetString(1)}");
    }
}

System.Console.WriteLine("\nMarking AddStoreCode migration as applied...");
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20260525153423_AddStoreCode', '10.0.8') ON CONFLICT (\"MigrationId\") DO NOTHING";
    cmd.ExecuteNonQuery();
}

System.Console.WriteLine("Updated migration history:");
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "SELECT \"MigrationId\", \"ProductVersion\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\"";
    using var reader2 = cmd.ExecuteReader();
    while (reader2.Read())
    {
        System.Console.WriteLine($"   {reader2.GetString(0)} | {reader2.GetString(1)}");
    }
}
