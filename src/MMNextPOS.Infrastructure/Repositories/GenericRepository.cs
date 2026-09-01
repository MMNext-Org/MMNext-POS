using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository for simple CRUD operations on master data.
    /// Suitable for entities with standard Id, Code, Name, IsActive fields.
    /// </summary>
    public class GenericRepository<T> : RepositoryBase, IRepository<T> where T : class
    {
        private readonly string _tableName;

        public GenericRepository(IUnitOfWork unitOfWork, string tableName) : base(unitOfWork)
        {
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            var sql = $"INSERT INTO {_tableName} SET ";
            var props = typeof(T).GetProperties();
            var columns = new List<string>();
            var parameters = new Dictionary<string, object?>();

            foreach (var prop in props)
            {
                if (prop.Name == "Id") continue; // Skip Id for insert

                columns.Add($"{prop.Name} = @{prop.Name}");
                parameters[prop.Name] = prop.GetValue(entity);
            }

            sql += string.Join(", ", columns);
            sql += "; SELECT LAST_INSERT_ID();";

            var id = await Connection.ExecuteScalarAsync<long>(sql, parameters, Transaction).ConfigureAwait(false);

            // Set the Id property
            var idProp = typeof(T).GetProperty("Id");
            if (idProp != null)
            {
                idProp.SetValue(entity, (int)id);
            }

            return entity;
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var sql = $"UPDATE {_tableName} SET ";
            var props = typeof(T).GetProperties();
            var updates = new List<string>();
            var parameters = new Dictionary<string, object?>();

            int? id = null;
            foreach (var prop in props)
            {
                if (prop.Name == "Id")
                {
                    id = (int?)prop.GetValue(entity);
                    continue;
                }

                updates.Add($"{prop.Name} = @{prop.Name}");
                parameters[prop.Name] = prop.GetValue(entity);
            }

            if (!id.HasValue)
                throw new InvalidOperationException("Entity must have an Id");

            sql += string.Join(", ", updates);
            sql += " WHERE Id = @Id";
            parameters["Id"] = id.Value;

            await Connection.ExecuteAsync(sql, parameters, Transaction).ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var sql = $"DELETE FROM {_tableName} WHERE Id = @Id";
            await Connection.ExecuteAsync(sql, new { Id = id }, Transaction).ConfigureAwait(false);
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id";
            return await Connection.QuerySingleOrDefaultAsync<T>(sql, new { Id = id }, Transaction).ConfigureAwait(false);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var sql = $"SELECT * FROM {_tableName}";
            var result = await Connection.QueryAsync<T>(sql, transaction: Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}
