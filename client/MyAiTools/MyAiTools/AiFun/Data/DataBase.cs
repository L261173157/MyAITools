using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAiTools.AiFun.Model;
using SQLite;

namespace MyAiTools.AiFun.Data
{
    public class DataBase
    {
        private SQLiteAsyncConnection? _database;

        private async Task Init<T>(T tableModel) where T : BaseModel, new()
        {
            try
            {
                if (_database is not null)
                    return;

                _database = new SQLiteAsyncConnection(DbConstants.DatabasePath, DbConstants.Flags);
                await _database.CreateTableAsync<T>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<T>> GetItemsAsync<T>(T tableModel) where T : BaseModel, new()
        {
            await Init(tableModel);
            return await _database.Table<T>().ToListAsync();
        }


        public async Task<T> GetItemAsync<T>(T tBaseModel, int id) where T : BaseModel, new()
        {
            await Init(tBaseModel);
            return await _database.Table<T>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveItemAsync<T>(T item) where T : BaseModel, new()
        {
            await Init(item);

            var result = await _database.InsertAsync(item);
            return result;
        }

        public async Task<int> UpdateItemAsync<T>(T item) where T : BaseModel, new()
        {
            await Init(item);
            return await _database.UpdateAsync(item);
        }

        public async Task<int> DeleteItemAsync<T>(T item) where T : BaseModel, new()
        {
            await Init(item);
            return await _database.DeleteAsync(item);
        }

        public async Task<int> DeleteAllAsync<T>(T item) where T : BaseModel, new()
        {
            await Init(item);
            return await _database.DeleteAllAsync<T>();
        }
    }
}