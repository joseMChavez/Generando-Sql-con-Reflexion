 public static class Utilitarios
    {
        /// <summary>
        /// Indica que esta propiedad no sera tomada en cuenta.
        /// </summary>
        public class Ignore : Attribute{}
        /// <summary>
        /// Indica que esta propiedad es un campo unico o identity.
        /// </summary>
        public class Identidad : Attribute { }
        /// <summary>
        /// Retorna la descripcion de una propiedad especificada
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objeto"></param>
        /// <returns></returns>
        public static string GetDescripcion<T>(this T objeto)
        {
            FieldInfo campoInfo = objeto.GetType().GetField(objeto.ToString());

            DescriptionAttribute[] atributos = (DescriptionAttribute[])campoInfo.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (atributos != null && atributos.Length > 0)
                return atributos[0].Description;

            return "None";
        }
        /// <summary>
        /// Auto Genera un Select apartir de un objeto
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objeto"></param>
        /// <param name="nombreTabla"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static string GetSqlSelect<T>(this T objeto,string nombreTabla, string where)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                Type type = objeto.GetType();
                var propertyInfo = type.GetProperties().Where(x => ((Ignore)x.GetCustomAttribute(typeof(Ignore), true)) == null && ((Identidad)x.GetCustomAttribute(typeof(Identidad), true)) == null );
                sql.Append($"SELECT ");
                string cadena = string.Empty;
                propertyInfo.ToList().ForEach(x => cadena += x.Name + ",");
                sql.Append(cadena.TrimEnd(','));
                sql.Append($" FROM {nombreTabla} " + (string.IsNullOrWhiteSpace(where) ? "" : $" WHERE {where}")+";");
                return sql.ToString();
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        
        /// <summary>
        /// Genera automaticamente un Insert, con el objeto y valores cargados a estos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objeto"></param>
        /// <param name="nombreTabla"></param>
        /// <returns></returns>
        public static string GetSqlInsert<T>(this T objeto, string nombreTabla)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                string cadena = string.Empty;
                object valor;

                Type type = objeto.GetType();
                var propertyInfo = type.GetProperties().Where(x => ((Ignore)x.GetCustomAttribute(typeof(Ignore), true)) == null && ((Identidad)x.GetCustomAttribute(typeof(Identidad), true)) == null);
                sql.Append($"INSERT INTO {nombreTabla}(");
              
                propertyInfo.ToList().ForEach(x => cadena += $"{x.Name},");
                sql.Append($"{cadena.TrimEnd(',')}) VALUES(");
                cadena = string.Empty;
                propertyInfo.ToList().ForEach(item=> {
                   valor = typeof(T).GetProperty(item.Name).GetValue(objeto, null);
                    if (valor != null && (valor is float || valor is int || valor is double || valor is decimal))
                        cadena += $"{valor} ,";
                    else
                    {
                        if (valor is DateTime)
                            cadena += $"'{valor:yyyy-MM-dd hh:mm:ss}',";
                        else
                            cadena += $"'{valor}',";
                    }
                });
                sql.Append(cadena.TrimEnd(',')+");");
                return sql.ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Genera un update del objeto y la table especificada
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objeto"></param>
        /// <param name="nombreTabla"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static string GetSqlUdate<T>(this T objeto, string nombreTabla, string where)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(where)) { return "Where es un parametro obligatorio!";}
                StringBuilder sql = new StringBuilder();
                string cadena = string.Empty;
                object valor;
                Type type = objeto.GetType();
                var propertyInfo = type.GetProperties().Where(x => ((Ignore)x.GetCustomAttribute(typeof(Ignore), true)) == null && ((Identidad)x.GetCustomAttribute(typeof(Identidad), true)) == null);
                sql.Append($" UPDATE {nombreTabla} SET ");
                
                propertyInfo.ToList().ForEach(item => {
                    cadena += $"{item.Name}=";
                    valor = typeof(T).GetProperty(item.Name).GetValue(objeto, null);
                    if (valor != null && (valor is float || valor is int || valor is double || valor is decimal))
                    {
                        cadena += $"{valor} ,";
                    }
                    else
                    {
                        if (valor is DateTime)
                            cadena += $"'{valor:yyyy-MM-dd hh:mm:ss}',";
                        else
                            cadena += $"'{valor}',";
                    }

                });
                
                sql.Append(cadena.TrimEnd(',') + $" WHERE {where};");

                return sql.ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Retorna en una lista, los valores y descripciones de las propiedades en un enums
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static List<ObjetoSelect> GetDescripcionEnums(Enum en)
        {

            List<ObjetoSelect> lista = new List<ObjetoSelect>();
            Type type = en.GetType();
            var lis = Enum.GetValues(type).Cast<Enum>().Select(value => new
           {
             (Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute).Description,
             value
            }).OrderBy(item => item.value).ToList();

            foreach (var item in lis)
            {
                lista.Add(new ObjetoSelect
                {
                    Value =((int)Enum.Parse(type,item.value.ToString())).ToString(),
                    Descripcion = item.Description
                }); 

            }

            return lista;
        }

    }
