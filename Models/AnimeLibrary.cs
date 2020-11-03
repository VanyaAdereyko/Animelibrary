using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Animelibrary.Models
{
    public class AnimeLibrary
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _Id { get; set; }
        [Display(Name = "Наименование")]
        public string _Name { get; set; }
        [Display(Name = "Описание")]
        public string _Description { get; set; }
        [Display(Name = "Дата выхода")]
        public DateTime _DataTime { get; set; }

        public string ImageId { get; set; } // ссылка на изображение

        public bool HasImage()
        {
            return !String.IsNullOrWhiteSpace(ImageId);
        }
    }
}

