using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository base that supplies a protected method to obtain a connection from the UnitOfWork.
    /// Concrete repositories can inherit from this to avoid repeating connection logic.
    /// </summary>
    public abstract class RepositoryBase
    {
        protected readonly IUnitOfWork _unitOfWork;

        protected RepositoryBase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        protected MySqlConnection Connection => _unitOfWork.Connection;

        protected MySqlTransaction? Transaction => _unitOfWork.Transaction;
    }
}
