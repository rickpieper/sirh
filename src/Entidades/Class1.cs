using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
namespace Entidades
{
    [Table("articles")]
    public class Artigo
    {
        [Column("id")]
        public int Id { get; set; }
      
        [Column("title")]
        public string Titulo { get; set; }
        
        public string Article { get; set; }
    
    }

    public static class Test
    {

        public static void Testm(MySql.Data.MySqlClient.MySqlConnection connection)
        {
            DataContext db = new DataContext(connection);

            Table<Artigo> Artigos = db.GetTable<Artigo>();

            var q = (from a in Artigos select a).ToList();
        
        }

    }


}
