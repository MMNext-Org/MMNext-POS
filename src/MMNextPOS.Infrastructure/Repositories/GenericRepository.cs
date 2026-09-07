using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly bool _hasIsDeleted;

        public GenericRepository(IUnitOfWork unitOfWork, string tableName) : base(unitOfWork)
        {
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _hasIsDeleted = typeof(T).GetProperty("IsDeleted") != null;
        }

        /// <summary>
        /// Returns true when the property maps to a database column.
        /// Navigation/complex/collection properties and [NotMapped] members are skipped so
        /// reflection-based INSERT/UPDATE statements only contain real columns.
        /// </summary>
        private static bool IsColumnProperty(PropertyInfo prop)
        {
            if (!prop.CanRead || !prop.CanWrite) return false;
            if (prop.GetIndexParameters().Length > 0) return false;
            if (prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() != null) return false;

            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(Guid)
                || type == typeof(byte[]);
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
                if (!IsColumnProperty(prop)) continue; // Skip navigation/complex properties

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
                if (!IsColumnProperty(prop)) continue; // Skip navigation/complex properties

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
            if (_hasIsDeleted)
            {
                // Soft delete
                var sql = $"UPDATE {_tableName} SET IsDeleted = 1 WHERE Id = @Id";
                await Connection.ExecuteAsync(sql, new { Id = id }, Transaction).ConfigureAwait(false);
            }
            else
            {
                // Hard delete
                var sql = $"DELETE FROM {_tableName} WHERE Id = @Id";
                await Connection.ExecuteAsync(sql, new { Id = id }, Transaction).ConfigureAwait(false);
            }
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id";
            if (_hasIsDeleted)
            {
                sql += " AND IsDeleted = 0";
            }
            return await Connection.QuerySingleOrDefaultAsync<T>(sql, new { Id = id }, Transaction).ConfigureAwait(false);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var sql = $"SELECT * FROM {_tableName}";
            if (_hasIsDeleted)
            {
                sql += " WHERE IsDeleted = 0";
            }
            var result = await Connection.QueryAsync<T>(sql, transaction: Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public virtual async Task<PagedResult<T>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var offset = (page - 1) * pageSize;

            var countSql = $"SELECT COUNT(*) FROM {_tableName}";
            if (_hasIsDeleted)
            {
                countSql += " WHERE IsDeleted = 0";
            }

            var totalCount = await Connection.ExecuteScalarAsync<int>(countSql, transaction: Transaction).ConfigureAwait(false);

            var sql = $"SELECT * FROM {_tableName}";
            if (_hasIsDeleted)
            {
                sql += " WHERE IsDeleted = 0";
            }
            sql += $" ORDER BY Id LIMIT @Limit OFFSET @Offset";

            var parameters = new { Limit = pageSize, Offset = offset };
            var result = await Connection.QueryAsync<T>(sql, parameters, Transaction).ConfigureAwait(false);

            return new PagedResult<T>
            {
                Items = result.AsList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
