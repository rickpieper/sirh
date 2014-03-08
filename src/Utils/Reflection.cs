using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
namespace BlackMagic
{

    //.NET Magic! 
    //Don't touch this.


    //Ricardo Pieper - 2014

    public class Reflector<T> : IDisposable
    {

        public void Dispose()
        {

            this.propDict.Clear();
            this.propDict = null;

            this.propList.Clear();
            this.propList = null;
        }

        public Type CurrentType { get; private set; }

        private List<ReflectorProperty> propList = new List<ReflectorProperty>();
        private Dictionary<PropertyInfo, ReflectorProperty> propDict = new Dictionary<PropertyInfo, ReflectorProperty>();

        public Reflector()
        {
            this.CurrentType = typeof(T);

            //construir lista de propriedades
            var properties = CurrentType.GetProperties();
            Properties = new List<PropertyInfo>();
            foreach (var prop in CurrentType.GetProperties())
            {
                var refProp = new ReflectorProperty(prop);
                Properties.Add(prop);
                propDict.Add(prop, refProp);
                propList.Add(refProp);
            }

        }



        public List<PropertyInfo> Properties { get; set; }

        public ReflectorProperty this[PropertyInfo property]
        {
            get
            {
                if (stringIndexes.ContainsKey(property.Name))
                {
                    return propList[stringIndexes[property.Name]];
                }

                if (propDict.ContainsKey(property))
                {
                    return propDict[property];
                }
                else return null;
            }
        }

        Dictionary<string, int> stringIndexes = new Dictionary<string, int>();

       


    }

    public class ReflectorProperty
    {
       
        public PropertyInfo Property { get; private set; }

        public ReflectorProperty(PropertyInfo property)
        {
            this.Property = property;
        }
        private Func<Object, Object> getterCache { get; set; }
        public Func<Object, Object> Getter
        {
            get
            {
                if (getterCache == null)
                {
                    var instance = Expression.Parameter(typeof(object), "instance"); //instance
                    var propertyExpr = Expression.Property(Expression.Convert(instance, Property.DeclaringType), Property); //((Foo)instance)).prop
                    //é necessario que o retorno seja em forma de objeto
                    var cast = Expression.Convert(propertyExpr, typeof(Object));

                    getterCache = Expression.Lambda<Func<Object, Object>>(cast, instance).Compile();
                }
                return getterCache;
            }
        }

        private Action<Object, Object> setterCache { get; set; }
        public Action<Object, Object> Setter
        {

            get
            {

                if (setterCache == null)
                {
                    var instance = Expression.Parameter(typeof(object), "instance");
                    var propertyExpr = Expression.Property(Expression.Convert(instance, Property.DeclaringType), Property);
                    var value = Expression.Parameter(Property.PropertyType, "value");
                    var assign = Expression.Assign(propertyExpr, value);
                    setterCache = Expression.Lambda<Action<Object, Object>>(assign, instance, value).Compile();
                }
                return setterCache;
            }

        }



    }

}
