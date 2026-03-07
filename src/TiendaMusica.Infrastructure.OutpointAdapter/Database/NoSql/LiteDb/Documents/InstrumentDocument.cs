using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiendaMusica.Domain.Models;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents
{
    public class InstrumentDocument
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
        public DateTime CreationDateUtc { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public InstrumentType Type { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
