using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        [Obsolete("Obsolete")]
        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
            var options = new CreateIndexOptions<UserEntity> { Unique = true };
            userCollection.Indexes.CreateOne("{ Login : 1 }", options);
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            var filter = new BsonDocument();
            using var cursor = userCollection.Find(filter).ToCursor();
            while (cursor.MoveNext())
            {
                foreach (var ue in cursor.Current)
                {
                    if (ue.Id == id)
                    {
                        return ue;
                    }
                }
            }
            return null;
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var filter = new BsonDocument();
            using var cursor = userCollection.Find(filter).ToCursor();
            while (cursor.MoveNext())
            {
                foreach (var ue in cursor.Current)
                {
                    if (ue.Login == login)
                    {
                        return ue;
                    }
                }
            }

            return Insert(new UserEntity { Login = login });
        }

        public void Update(UserEntity user)
        {
            userCollection.FindOneAndReplace(u => u.Id == user.Id, user);
        }

        public void Delete(Guid id)
        {
            var filter = new BsonDocument();
            userCollection.DeleteOne(filter);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var list = userCollection.Find(new BsonDocument()).SortBy(u => u.Login).Skip((pageNumber-1)*pageSize).Limit(pageSize).ToList();
            return new PageList<UserEntity>(list, userCollection.Find(new BsonDocument()).CountDocuments(), pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}