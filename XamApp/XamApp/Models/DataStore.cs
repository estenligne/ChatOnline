using System;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace XamApp.Models
{
    public class DataStore
    {
        public static DataStore Instance = new DataStore();

        private SQLiteAsyncConnection database;

        public DataStore()
        {
            if (database == null)
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var databasePath = System.IO.Path.Combine(basePath, "ChatOnline_SQLite.db3");

                var flags =
                    SQLiteOpenFlags.ReadWrite | // open the database in read/write mode
                    SQLiteOpenFlags.Create | // create the database if it doesn't exist
                    SQLiteOpenFlags.SharedCache; // enable multi-threaded database access

                database = new SQLiteAsyncConnection(databasePath, flags);

                // Create database tables
                database.CreateTableAsync<User>();
            }
        }

        public Task<int> UpdateUserAsync(User user)
        {
            return database.UpdateAsync(user);
        }

        public Task<int> InsertUserAsync(User user)
        {
            return database.InsertAsync(user);
        }

        public Task<int> DeleteUserAsync()
        {
            return database.DeleteAllAsync<User>();
        }

        public Task<User> GetUserAsync()
        {
            return database.Table<User>().FirstOrDefaultAsync();
        }
    }
}
