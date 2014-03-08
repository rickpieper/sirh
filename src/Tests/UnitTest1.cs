using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils;
using System.Collections.Generic;
using System.Linq;
using Matrix = System.Collections.Generic.List<System.Collections.Generic.List<int>>;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [Table("Articles")]
        public class Article
        {
            [Key]
            public int Id { get; set; }
            public string Title { get; set; }
            [Column("article")]
            public string TextArticle { get; set; }
        }


        public static IEnumerable<IEnumerable<int>> SomaMatrizes(Matrix mtx1, Matrix mtx2)
        {
            return mtx1.Select((ls, x) => ls.Select((ls2, y) => mtx2[x][y] += mtx1[x][y]));
        }



        public void TestaSeDaCerto()
        {

            var matriz1 = new List<List<int>>{
                    new List<int>{ 1,2,3,4,5,6 },
                    new List<int>{ 1,2,3,4,5,6 },
                    new List<int>{ 1,2,3,4,5,6 },
                    new List<int>{ 1,2,3,4,5,6 },
                    new List<int>{ 1,2,3,4,5,6 },
                };

            var x = SomaMatrizes(matriz1,
                   new List<List<int>>{
                    new List<int>{ 1,2,3,4,5,6 },
                    new List<int>{ 1,2,3,4,5,6 },
                    new List<int>{ 1,2,3,4,5,6 },
                    new List<int>{ 1,2,3,4,5,6 },
                    new List<int>{ 1,2,3,4,5,6 },
                });
        }


        //  [TestMethod]
        public void QueryWithoutParameters()
        {
            Query q = new Query("select * from articles");

            /* var results = q.ExecuteQuery().MapTo<Texto>(item => new Texto
             {
                 Id = item.Int32("id"),
                 Titulo = item.String("title"),
                 Conteudo = item.String("article")
             }).ToList();
             */

        }


        // [TestMethod]
        public void QueryWithParameters()
        {
            /* Query q = new Query();
             string sql = "select * from articles where 1=1";

             int id = 5;
             if (id != 0)
             {
                 sql += " and id = @id";
                 q.AddParameter("id", id);
             }
             */
            /*  List<Texto> articles = q.ExecuteQuery(sql).MapTo(x => new Texto
              {
                  Id = x.Int32("id"),
                  Titulo = x.String("title"),
                  Conteudo = x.String("article")

              }).ToList();
              */
            //Assert.IsTrue(articles.Count > 0);

        }

        //    [TestMethod]
        public void InsertParameters()
        {

            Query.Insert<Article>(new Article
            {
                TextArticle = @"Noticia criada automaticamente uheuehueaheuaheuaehaueheauehauehauehauehauehe",
                Title = "Noticia C#"
            });

        }
        [TestMethod]
        public void Update()
        {
            Query.Update<Article>(new Article
            {
                Id = 13,
                TextArticle = "HUEHUEHEUE",
                Title = "Hue."
            });




        }


    }
}
