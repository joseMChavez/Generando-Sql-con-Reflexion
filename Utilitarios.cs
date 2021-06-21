 public static class GenerateSqlScript
    {
        /// <summary>
        /// Atributo tabla
        /// </summary>
        public class Tabla : Attribute
        {
            /// <summary>
            /// Nombre de la tabla
            /// </summary>
            public string Nombre { get; set; }
            /// <summary>
            /// Schema al que pertenece la tabla
            /// </summary>
            public string Schema { get; set; }
            public Tabla(string nombre, string schema)
            {
                Nombre = nombre;
                Schema = schema;
            }
            public Tabla(string nombre)
            {
                Nombre = nombre;
                Schema = string.Empty;
            }
            public Tabla()
            {
                Nombre = string.Empty;
                Schema = string.Empty;
            }
           
        }

        /// <summary>
        /// Indica que esta propiedad no sera tomada en cuenta.
        /// </summary>
        public class Ignore : Attribute{}
        /// <summary>
        /// Indica que esta propiedad es un campo unico o identity.
        /// </summary>
        public class Identidad : Attribute { }
    
        /// <summary>
        /// Auto Genera un Select apartir de un objeto
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objeto"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static string GetSqlSelect<T>(this T objeto, string where)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                Type type = objeto.GetType();
                 _ = new Tabla();
                Tabla tabla = GetInstanciaTabla<T>(objeto);
                string nombreTabla = $"{(!string.IsNullOrWhiteSpace(tabla.Schema)?$"{tabla.Schema}.":"")} {tabla.Nombre}";
                if (string.IsNullOrWhiteSpace(nombreTabla))
                {
                    nombreTabla = type.Name+"s";
                }
                var propertyInfo = type.GetProperties().Where(x => ((Ignore)x.GetCustomAttribute(typeof(Ignore), true)) == null);
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
        /// <returns></returns>
        public static string GetSqlInsert<T>(this T objeto)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                string cadena = string.Empty;
                object valor;

                Type type = objeto.GetType();
                 _ = new Tabla();
                Tabla tabla = GetInstanciaTabla<T>(objeto);
                string nombreTabla = $"{(!string.IsNullOrWhiteSpace(tabla.Schema)?$"{tabla.Schema}.":"")} {tabla.Nombre}";
                if (string.IsNullOrWhiteSpace(nombreTabla))
                {
                    nombreTabla = type.Name+"s";
                }
                var propertyInfo = type.GetProperties().Where(x => ((Ignore)x.GetCustomAttribute(typeof(Ignore), true)) == null && ((Identidad)x.GetCustomAttribute(typeof(Identidad), true)) == null);
                sql.Append($"INSERT INTO {nombreTabla}(");
              
                propertyInfo.ToList().ForEach(x => cadena += $"{x.Name},");
                sql.Append($"{cadena.TrimEnd(',')}) VALUES(");
                cadena = string.Empty;
                propertyInfo.ToList().ForEach(item=> {
                   valor = typeof(T).GetProperty(item.Name).GetValue(objeto, null);
                    if (valor != null && (valor is float || valor is int || valor is double || valor is decimal || valor is Enum))
                        cadena += $"{valor} ,";
                    else if(valor != null)
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
        /// <param name="where"></param>
        /// <returns></returns>
        public static string GetSqlUpdate<T>(this T objeto, string where)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(where)) { return "Where es un parametro obligatorio!";}
                StringBuilder sql = new StringBuilder();
                string cadena = string.Empty;
                object valor;
                Type type = objeto.GetType();
                 _ = new Tabla();
                Tabla tabla = GetInstanciaTabla<T>(objeto);
                string nombreTabla = $"{(!string.IsNullOrWhiteSpace(tabla.Schema)?$"{tabla.Schema}.":"")} {tabla.Nombre}";
                if (string.IsNullOrWhiteSpace(nombreTabla))
                {
                    nombreTabla = type.Name+"s";
                }
                var propertyInfo = type.GetProperties().Where(x => ((Ignore)x.GetCustomAttribute(typeof(Ignore), true)) == null && ((Identidad)x.GetCustomAttribute(typeof(Identidad), true)) == null);
                sql.Append($" UPDATE {nombreTabla} SET ");
                
                propertyInfo.ToList().ForEach(item => {
                    cadena += $"{item.Name}=";
                    valor = typeof(T).GetProperty(item.Name).GetValue(objeto, null);
                    if (valor != null && (valor is float || valor is int || valor is double || valor is decimal || valor is Enum))
                    {
                        cadena += $"{valor} ,";
                    }
                    else if(valor != null)
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
        /// Retorna el exec de un procedimiento almacenado, con los valores cargados...
        /// Es importante que los parametros se llamen igual que las propiedades de los objetos para que se pueda hacer merge...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objeto"></param>
        /// <param name="NombreSp"></param>
        /// <returns></returns>
        public static string GetStringExecSp<T>(this T objeto,string NombreSp)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                string cadena = string.Empty;
                object valor;

                Type type = objeto.GetType();
                var propertyInfo = type.GetProperties().Where(x => ((Ignore)x.GetCustomAttribute(typeof(Ignore), true)) == null);
                sql.Append($" Exec  {NombreSp} ");
                propertyInfo.ToList().ForEach(item => {
                    cadena += $"@{item.Name}=";
                    valor = typeof(T).GetProperty(item.Name).GetValue(objeto, null);
                    if (valor != null && (valor is float || valor is int || valor is double || valor is decimal || valor is Enum))
                    {
                        cadena += $"{valor},";
                    }
                    else if (valor != null)
                    {
                        if (valor is DateTime)
                            cadena += $"'{valor:yyyy-MM-dd hh:mm:ss}',";
                        else
                            cadena += $"'{valor}',";
                    }

                });

                sql.Append($"{cadena.TrimEnd(',')};");

                return sql.ToString();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
      

    }
