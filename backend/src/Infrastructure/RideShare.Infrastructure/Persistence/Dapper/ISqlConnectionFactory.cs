using System.Data;

namespace RideShare.Infrastructure.Persistence.Dapper;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
