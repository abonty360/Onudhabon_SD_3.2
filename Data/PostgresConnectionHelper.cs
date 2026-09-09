using System;
using Npgsql;

namespace Onudhabon.Data
{
    public static class PostgresConnectionHelper
    {
        public static string ConvertUrlToConnectionString(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(raw);
                var userInfo = uri.UserInfo.Split(':');
                var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
                var database = uri.AbsolutePath.TrimStart('/');

                var npgsqlBuilder = new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port > 0 ? uri.Port : 5432,
                    Username = username,
                    Password = password,
                    Database = database,
                    SslMode = SslMode.Require,
                    Timeout = 60,
                    CommandTimeout = 60,
                    IncludeErrorDetail = true
                };

                if (!string.IsNullOrWhiteSpace(uri.Query))
                {
                    var query = uri.Query.TrimStart('?');
                    var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var pair in pairs)
                    {
                        var kv = pair.Split('=', 2);
                        var key = Uri.UnescapeDataString(kv[0]).ToLowerInvariant();
                        var val = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty;

                        if (key == "sslmode" && Enum.TryParse<SslMode>(val, true, out var sslMode))
                        {
                            npgsqlBuilder.SslMode = sslMode;
                        }
                        else if (key == "channel_binding" && Enum.TryParse<ChannelBinding>(val, true, out var cb))
                        {
                            // PgBouncer pooler terminates TLS at proxy level where SCRAM channel-binding
                            // requirement causes reading timeouts. Use Prefer when connecting via pooler.
                            npgsqlBuilder.ChannelBinding = uri.Host.Contains("-pooler", StringComparison.OrdinalIgnoreCase) 
                                ? ChannelBinding.Prefer 
                                : cb;
                        }
                    }
                }

                return npgsqlBuilder.ConnectionString;
            }

            return raw;
        }
    }
}
