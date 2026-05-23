using System;
using ByteBite.Shared.Helpers;
using Npgsql;

var connStr = "Host=192.168.3.22;Port=5432;Database=kongkong_bytebite;Username=konghao;Password=hitek.123";
using var conn = new NpgsqlConnection(connStr);
conn.Open();

var hash = PasswordHasher.HashPassword("123456");
System.Console.WriteLine($"Generated hash for '123456': {hash}");

using var cmd = conn.CreateCommand();
cmd.CommandText = @"
    INSERT INTO admins (username, password_hash, display_name, role, status) 
    VALUES ('admin', @hash, '管理员', 'super_admin', 'active')
    ON CONFLICT (username) DO UPDATE SET password_hash = @hash";
cmd.Parameters.AddWithValue("hash", hash);
var rows = cmd.ExecuteNonQuery();
System.Console.WriteLine($"Upserted {rows} admin row(s)");

using var cmd2 = conn.CreateCommand();
cmd2.CommandText = "SELECT id, username, role, status FROM admins WHERE username = 'admin'";
using var reader = cmd2.ExecuteReader();
while (reader.Read())
{
    System.Console.WriteLine($"Admin: id={reader.GetGuid(0)}, username={reader.GetString(1)}, role={reader.GetString(2)}, status={reader.GetString(3)}");
}
