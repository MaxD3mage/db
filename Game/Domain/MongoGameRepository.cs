using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Game.Domain
{
    // TODO Сделать по аналогии с MongoUserRepository
    public class MongoGameRepository : IGameRepository
    {
        public const string CollectionName = "games";
        private readonly IMongoCollection<GameEntity> gameCollection;

        public MongoGameRepository(IMongoDatabase db)
        {
            gameCollection = db.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            gameCollection.InsertOne(game);
            return game;
        }

        public GameEntity FindById(Guid gameId)
        {
            var filter = new BsonDocument();
            using var cursor = gameCollection.Find(filter).ToCursor();
            while (cursor.MoveNext())
            {
                foreach (var ue in cursor.Current)
                {
                    if (ue.Id == gameId)
                    {
                        return ue;
                    }
                }
            }
            return null;
        }

        public void Update(GameEntity game)
        {
            gameCollection.FindOneAndReplace(u => u.Id == game.Id, game);
        }

        // Возвращает не более чем limit игр со статусом GameStatus.WaitingToStart
        public IList<GameEntity> FindWaitingToStart(int limit)
        {
            return gameCollection.Find(h => h.Status == GameStatus.WaitingToStart).Limit(limit).ToList();
        }

        // Обновляет игру, если она находится в статусе GameStatus.WaitingToStart
        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            var res = gameCollection.ReplaceOne(h => h.Status == GameStatus.WaitingToStart && h.Id == game.Id, game);
            return res.IsAcknowledged && res.ModifiedCount != 0;
        }
    }
}