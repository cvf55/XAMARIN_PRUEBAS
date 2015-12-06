using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Data.Common;

namespace WSAndroid {
    internal class cControlLog : cControlBDLocal {
        #region "Variables privadas"
        //Para implementar SINGLENTON
        private static cControlLog m_Instancia = null; //Para Implementar el Singleton
        private static object m_Sync = new object(); //Para que la creacion del objeto sea seguro en multi hilo
        #endregion

        #region "Eventos y delegados"
        public override event TareaCompletadaHandler TareaCompletadaEvent;
        #endregion

        #region "Constructores"
        private cControlLog(string NombreBD, ILog cLog = null) : base(NombreBD, cLog) { }
        #endregion

        #region "Metodos publicos"
        /// <summary>
        /// Implementacion de SINGLETON. Unico punto de entrada a la clase
        /// </summary>
        /// <param name="NombreBD">Nombre de la base de datos a crear</param>
        /// <param name="cLog">Control del log</param>
        /// <returns></returns>
        public static cControlLog Instancia(string NombreBD, ILog cLog = null) {
            if (m_Instancia == null) {
                lock (m_Sync) {
                    if (m_Instancia == null) {
                        m_Instancia = new cControlLog(NombreBD, cLog);
                    }
                }
            }
            return m_Instancia;
        }

        /// <summary>
        /// Creamos la tablas necesarias
        /// </summary>
        public override void CrearTablas() {
            StringBuilder sSQL = new StringBuilder(1000);
            DbCommand cmdComando;
            object Bloqueo = new object();

            this.TareaCompletadaEvent(this, "Creando tabla ZLOG");

            sSQL.AppendLine("CREATE TABLE IF NOT EXISTS ZLOG (");
            sSQL.AppendLine("CODIGO INTEGER PRIMARY KEY AUTOINCREMENT,");
            sSQL.AppendLine("FECHA TEXT DEFAULT '1900-01-01 00:00:00',");
            sSQL.AppendLine("GRAVEDAD INTEGER DEFAULT 1 CHECK (GRAVEDAD IN (1,2,3,4)),"); //1 - INFORMACION, 2 - ADVERTENCIA, 3 - ERROR, 4 - DEBUG
            sSQL.AppendLine("MODULO TEXT DEFAULT '', ");
            sSQL.AppendLine("FUNCION TEXT DEFAULT '', ");
            sSQL.AppendLine("LINEA INTEGER, ");
            sSQL.AppendLine("MENSAJE1 TEXT DEFAULT '', ");
            sSQL.AppendLine("MENSAJE2 TEXT DEFAULT '', ");
            sSQL.AppendLine("EXCEPCION TEXT DEFAULT '', ");
            sSQL.AppendLine("LEIDO INTEGER DEFAULT 1 CHECK (LEIDO IN (1,2))"); //1 - No, 2 - Si
            sSQL.AppendLine(")");

            //Creamos la tabla de log.
            //Bloqueamos esta parte del codigo. 
            lock (Bloqueo) {
                using (DbConnection cnSqlite = this.cnValida()) {
                    cmdComando = cnSqlite.CreateCommand();
                    cmdComando.CommandText = sSQL.ToString();

                    try {
                        cnSqlite.Open();
                        cmdComando.ExecuteNonQuery();
                    } catch (Exception ex) {
                        this.Log.GrabarMensaje(ex.Message, ex);
                        throw new xvkExcepcion("Error al crear la tabla ZLOG", ex);
                    }
                }
            }

            this.TareaCompletadaEvent(this, "Tabla ZLOG creada.");
        }

        /// <summary>
        /// Limpiamos las tablas eliminando los registros antiguos. Hay que mantener la base de datos lo mas pequeña posible.
        /// <param name="DiasAConservar">Numero de dias a conservar</param>
        /// </summary>
        public override void LimpiarBD(int DiasAConservar = 7) {
            StringBuilder sSql = new StringBuilder(1000);
            DbCommand cmdComando;
            object Bloqueo = new object();

            sSql.AppendLine("DELETE FROM ZLOG WHERE FECHA <= '" + DateTime.Today.AddDays(DiasAConservar * (-1)).ToString("yyyy-MM-dd 00:00:00") + "'");
            
            //Bloqueamos esta parte del codigo. 
            lock (Bloqueo) {
                using (DbConnection cnSQLITE = this.cnValida()) {
                    cmdComando = cnSQLITE.CreateCommand();
                    cmdComando.CommandText = sSql.ToString();

                    try {
                        cnSQLITE.Open();
                        cmdComando.ExecuteNonQuery();
                    } catch (Exception ex) {
                        this.Log.GrabarMensaje(ex.Message, ex);
                        throw new xvkExcepcion("Error al limpiar la tabla ZLOG", ex);
                    }
                }
            }
        } 
    }
    #endregion
}