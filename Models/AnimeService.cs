using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Animelibrary.Models
{
    public class AnimeService
    {
        IGridFSBucket gridFS;   // файловое хранилище
        IMongoCollection<AnimeLibrary> AnimeLibrarys; // коллекция в базе данных
        public AnimeService()
        {
            // строка подключения
            string connectionString = "mongodb://localhost:27017/mobilestore";
            var connection = new MongoUrlBuilder(connectionString);
            // получаем клиента для взаимодействия с базой данных
            MongoClient client = new MongoClient(connectionString);
            // получаем доступ к самой базе данных
            IMongoDatabase database = client.GetDatabase(connection.DatabaseName);
            // получаем доступ к файловому хранилищу
            gridFS = new GridFSBucket(database);
            // обращаемся к коллекции Products
            AnimeLibrarys = database.GetCollection<AnimeLibrary>("AnimeLibrary");
        }
        // получаем все документы, используя критерии фальтрации
        public async Task<IEnumerable<AnimeLibrary>> GetLibrary(int? minPrice, int? maxPrice, string name)
        {
            // строитель фильтров
            var builder = new FilterDefinitionBuilder<AnimeLibrary>();
            var filter = builder.Empty; // фильтр для выборки всех документов
            // фильтр по имени
            if (!String.IsNullOrWhiteSpace(name))
            {
                filter = filter & builder.Regex("Name", new BsonRegularExpression(name));
            }
            if (minPrice.HasValue)  // фильтр по минимальной цене
            {
                filter = filter & builder.Gte("Price", minPrice.Value);
            }
            if (maxPrice.HasValue)  // фильтр по максимальной цене
            {
                filter = filter & builder.Lte("Price", maxPrice.Value);
            }

            return await AnimeLibrarys.Find(filter).ToListAsync();
        }

        // получаем один документ по id
        public async Task<AnimeLibrary> GetLibrary(string id)
        {
            return await AnimeLibrarys.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }
        // добавление документа
        public async Task Create(AnimeLibrary p)
        {
            await AnimeLibrarys.InsertOneAsync(p);
        }
        // обновление документа
        public async Task Update(AnimeLibrary p)
        {
            await AnimeLibrarys.ReplaceOneAsync(new BsonDocument("_id", new ObjectId(p._Id)), p);
        }
        // удаление документа
        public async Task Remove(string id)
        {
            await AnimeLibrarys.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
        }
        // получение изображения
        public async Task<byte[]> GetImage(string id)
        {
            return await gridFS.DownloadAsBytesAsync(new ObjectId(id));
        }
        // сохранение изображения
        public async Task StoreImage(string id, Stream imageStream, string imageName)
        {
            AnimeLibrary p = await GetLibrary(id);
            if (p.HasImage())
            {
                // если ранее уже была прикреплена картинка, удаляем ее
                await gridFS.DeleteAsync(new ObjectId(p.ImageId));
            }
            // сохраняем изображение
            ObjectId imageId = await gridFS.UploadFromStreamAsync(imageName, imageStream);
            // обновляем данные по документу
            p.ImageId = imageId.ToString();
            var filter = Builders<AnimeLibrary>.Filter.Eq("_id", new ObjectId(p._Id));
            var update = Builders<AnimeLibrary>.Update.Set("ImageId", p.ImageId);
            await AnimeLibrarys.UpdateOneAsync(filter, update);
        }
    }
}
