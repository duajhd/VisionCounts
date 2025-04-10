using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Weighting
{
    public class DatabaseHelper : IDisposable
    {
        private readonly string _connectionString;
        private SQLiteConnection _connection;
        private SQLiteTransaction _transaction;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        public void Open()
        {
            if (_connection == null)
                _connection = new SQLiteConnection(_connectionString);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void Close()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
                _connection.Close();
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            Open();
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            _transaction?.Commit();
            _transaction = null;
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction = null;
        }

        /// <summary>
        /// 执行非查询命令（INSERT/UPDATE/DELETE）
        /// </summary>
        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                Open();
                using var cmd = new SQLiteCommand(sql, _connection);
                if (_transaction != null) cmd.Transaction = _transaction;
                AddParameters(cmd, parameters);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExecuteNonQuery Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 执行查询命令，返回 DataTable
        /// </summary>
        public DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                Open();
                using var cmd = new SQLiteCommand(sql, _connection);
                if (_transaction != null) cmd.Transaction = _transaction;
                AddParameters(cmd, parameters);
                using var adapter = new SQLiteDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExecuteQuery Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 执行查询返回单个值
        /// </summary>
        public object ExecuteScalar(string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                Open();
                using var cmd = new SQLiteCommand(sql, _connection);
                if (_transaction != null) cmd.Transaction = _transaction;
                AddParameters(cmd, parameters);
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExecuteScalar Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        private void AddParameters(SQLiteCommand cmd, Dictionary<string, object> parameters)
        {
            if (parameters == null) return;
            foreach (var kv in parameters)
            {
                cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }
}
