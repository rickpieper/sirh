using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class SqlBuilder
    {





    }
    public class Command
    {
        public string TableName;
    }

    public class InsertCommand : Command
    {
        public Dictionary<string, object> Values { get; set; }

    }
    public class UpdateCommand : Command
    {
        public WhereStatement Keys { get; set; }
        public Dictionary<string, object> Values { get; set; }

    }
    public class DeleteCommand : Command
    {
        public WhereStatement Where { get; set; }
    }


    public enum Operator
    {
        And, Or, Between
    }

    public enum Modifier
    {
        Not
    }

    public class WhereStatement : Command
    {
    
        
        
        private class Where
        {
            public string Sentence { get; set; }
            public Where Next { get; set; }
            public List<Where> Nested { get; set; }
            public Operator NextOperator { get; set; }

        }

        List<Where> Wheres { get; set; }

        public WhereStatement(string sentence)
        {
            Wheres = new List<Where>();

            Wheres.Add(new Where
            {
                Sentence = sentence
            });

        }


        public WhereStatement And(string sentence)
        {
            Where @new = new Where();
            
            int index = Wheres.Count;
            Wheres[index].Next = @new;
            Wheres[index].NextOperator = Operator.And;

            Wheres.Add(@new);

            return this;
        }

        public WhereStatement Or(string sentence)
        {
            Where @new = new Where();

            int index = Wheres.Count;
            Wheres[index].Next = @new;
            Wheres[index].NextOperator = Operator.And;

            Wheres.Add(@new);

            return this;
        }

        public WhereStatement Between(string sentence)
        {
            Where @new = new Where();

            int index = Wheres.Count;
            Wheres[index].Next = @new;
            Wheres[index].NextOperator = Operator.Between;

            Wheres.Add(@new);

            return this;
        }

    }



}
