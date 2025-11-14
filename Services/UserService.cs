using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Lyra.Services
{
    public class UserService
    {
        private readonly string? _connectionString;

        public UserService(IConfiguration config)
        {
            _connectionString =
                config.GetConnectionString("OracleConnection")
                ?? throw new Exception("Connection string 'OracleConnection' não encontrada.");
        }

        public async Task InserirUsuario(string nome, string email, string senha, string experience)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql =
                @"BEGIN 
                            pkg_user.inserir_usuario(:p_name, :p_email, :p_password, :p_experience_level);
                        END;";

            await conn.ExecuteAsync(
                sql,
                new
                {
                    p_name = nome,
                    p_email = email,
                    p_password = senha,
                    p_experience_level = experience,
                }
            );
        }
    }
}
