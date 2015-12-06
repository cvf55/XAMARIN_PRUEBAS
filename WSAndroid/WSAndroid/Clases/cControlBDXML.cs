using System;
using System.Collections;
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
    internal class cControlBDXML : cControlBDLocal, IEnumerable<struErroresSinc> {
        #region "Variables privadas"
        private Queue<struErroresSinc> m_ListaErrores; //Almacenamos los errores en una cola
        private bool m_HayErorres; //Indica si hay errroes.
        private string m_RutaCompletaBD; //Ruta completa del fichero de base de datos de errores en la sincronizacion.
        private ILog m_Log; //Control de log

        //Para implementar SINGLENTON
        private static cControlBDXML m_Instancia = null; //Para Implementar el Singleton
        private static object m_Sync = new object(); //Para que la creacion del objeto sea seguro en multi hilo
        #endregion

        #region "Eventos y delegados"
        public override event TareaCompletadaHandler TareaCompletadaEvent;
        #endregion

        #region "Propiedades"
        public bool HayErrores {
            get { return m_HayErorres; }
        }

        /// <summary>
        /// Implementacion de SINGLETON. Unico punto de entrada a la clase
        /// </summary>
        /// <param name="NombreBD">Nombre de la base de datos a crear</param>
        /// <param name="cLog">Control del log</param>
        /// <returns></returns>
        public static cControlBDXML Instancia(string NombreBD, ILog cLog = null) {
            if (m_Instancia == null) {
                lock (m_Sync) {
                    if (m_Instancia == null) {
                        m_Instancia = new cControlBDXML(NombreBD, cLog);
                    }
                }
            }
            return m_Instancia;
        }
        #endregion

        #region "Contructor"
        private cControlBDXML(string NombreBD, ILog cLog = null) : base(NombreBD, cLog) { Reset(); }
        #endregion

        #region "Funciones publicas"
        public IEnumerator<struErroresSinc> GetEnumerator() {
            return m_ListaErrores.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return m_ListaErrores.GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Quita todos los errores
        /// </summary>
        public void Reset() {
            m_ListaErrores = new Queue<struErroresSinc>();
            m_HayErorres = false;
        }

        /// <summary>
        /// Añade un error a la coleccion de errores
        /// </summary>
        /// <param name="Modulo"></param>
        /// <param name="Funcion"></param>
        /// <param name="Mensaje"></param>
        /// <param name="NombreWS"></param>
        /// <param name="XMLError"></param>
        public void Añadir(string Modulo, string Funcion, string Mensaje, string NombreWS, string XMLError) {
            StringBuilder sSql = new StringBuilder(1000);
            DbCommand cmdComando;
            struErroresSinc strError;
            DbTransaction Transaccion;
            object Bloqueo = new object();

            m_HayErorres = true;
            strError.Codigo = DateTime.Now.ToString("yyyyMMdd hh:mm:ss.ff");
            strError.Fecha = DateTime.Now;
            strError.Modulo = Modulo;
            strError.Funcion = Funcion;
            strError.Mensaje = Mensaje;
            strError.NombreWS = NombreWS;
            strError.XMLError = XMLError;

            lock (Bloqueo) {
                using (DbConnection cnLite = this.cnValida()) {
                    cmdComando = cnLite.CreateCommand();
                    Transaccion = cnLite.BeginTransaction();
                    cmdComando.Transaction = Transaccion;
                    cnLite.Open();

                    try {
                        //Insertamos en la cabecera
                        sSql.Clear();
                        sSql.AppendLine("INSERT OR REPLACE INTO CONTROLSYNC(CODIGO,FECHA,NOMBREWS,XMLERROR) VALUES ( ");
                        sSql.AppendLine("'" + strError.Codigo + "', ");
                        sSql.AppendLine("'" + strError.Fecha.ToString("yyyy-MM-dd hh:mm:ss.ff") + "', ");
                        sSql.AppendLine("'" + strError.NombreWS + "', ");
                        sSql.AppendLine("'" + strError.XMLError + "' ");
                        sSql.AppendLine(")");

                        cmdComando.CommandText = sSql.ToString();
                        cmdComando.ExecuteNonQuery();
                    } catch (Exception ex) {
                        Transaccion.Rollback();
                        m_Log.GrabarMensaje("Imposible insertar la cabecera del error", ex);
                        TareaCompletadaEvent(m_Instancia, ex.Message);
                        throw new xvkExcepcion("Error al insertar el error.", ex, m_Log);
                    }

                    try {
                        sSql.Clear();
                        sSql.AppendLine("INSERT OR REPLACE INTO CONTROLSYNCD(CODIGO,LINEA,FUNCION,MODULO,MENSAJE) VALUES (");
                        sSql.AppendLine("'" + strError.Codigo + "', ");
                        sSql.AppendLine(SigNumLinea(strError.Codigo).ToString() + ", ");
                        sSql.AppendLine("'" + strError.Funcion + "', ");
                        sSql.AppendLine("'" + strError.Modulo + "', ");
                        sSql.AppendLine("'" + strError.Mensaje + "'");
                        sSql.AppendLine(")");

                        cmdComando.CommandText = sSql.ToString();
                        cmdComando.ExecuteNonQuery();

                        Transaccion.Commit();
                    } catch (Exception ex) {
                        Transaccion.Rollback();
                        m_Log.GrabarMensaje("Imposible insertar el detalle del error", ex);
                        TareaCompletadaEvent(m_Instancia, ex.Message);
                        throw new xvkExcepcion("Error al insertar el error.", ex, m_Log);
                    }
                }
            }
            m_ListaErrores.Enqueue(strError);
        }

        /// <summary>
        /// Crear todas las tablas de la base de datos necesarias.
        /// </summary>
        public override void CrearTablas() {
            try {
                //Tabla: CONTROLSYNC
                CrearTablaCONTROLSYNC();
                //Tabla CONTROLSYNCD
                CrearTablaCONTROLSYNCD();
            } catch (Exception ex) {
                m_Log.GrabarMensaje("Error: " + ex.Message, ex);
                TareaCompletadaEvent(m_Instancia, ex.Message);
                throw new xvkExcepcion("Error al crear las tablas.", ex, m_Log);
            }
        }

        /// <summary>
        /// Limpiamos las tablas eliminando los registros antiguos. Hay que mantener la base de datos lo mas pequeña posible.
        /// <param name="DiasAConservar">Numero de dias a conservar</param>
        /// </summary>
        public override void LimpiarBD(int DiasAConservar = 7) {
            StringBuilder sSql = new StringBuilder(1000);
            DbCommand cmdComando;
            DbTransaction Transaccion;
            object Bloqueo = new object();

            //Bloqueo
            lock (Bloqueo) {
                using (DbConnection cnLite = this.cnValida()) {
                    cmdComando = cnLite.CreateCommand();
                    cnLite.Open();
                    Transaccion = cnLite.BeginTransaction();
                    cmdComando.Transaction = Transaccion;

                    try {
                        //Borramos la tabla de detalle
                        sSql.AppendLine("DELETE FROM CONTROLSYND WHERE CODIGO IN (SELECT CODIGO FROM CONTROLSYN WHERE FECHA <= '" + DateTime.Today.AddDays(DiasAConservar * (-1)).ToString("yyyy-MM-dd 00:00:00") + ")'");
                        cmdComando.CommandText = sSql.ToString();
                        cmdComando.ExecuteNonQuery();

                        //Borramos la cabecera
                        sSql.AppendLine("DELETE FROM CONTROLSYN WHERE FECHA <= '" + DateTime.Today.AddDays(DiasAConservar * (-1)).ToString("yyyy-MM-dd 00:00:00") + "'");
                        cmdComando.CommandText = sSql.ToString();
                        cmdComando.ExecuteNonQuery();

                        Transaccion.Commit();
                    } catch (Exception ex) {
                        Transaccion.Rollback();
                        m_Log.GrabarMensaje("Error: " + ex.Message, ex);
                        TareaCompletadaEvent(m_Instancia, ex.Message);
                        throw new xvkExcepcion("Error al limpiar las tablas.", ex, m_Log);
                    }
                }
            }
        }

        #region "Funciones privadas"
        /// <summary>
        /// Siguiente numero de linea para el error dado.
        /// </summary>
        /// <param name="sCodigoError">Codigo de error</param>
        /// <returns>Siguiente numero de linea para el error indicado.</returns>
        private int SigNumLinea(string sCodigoError) {
            //Nos conectamos a la base de datos y consultamos la linea
            string sSql;
            DbCommand cmdComando;
            int nContador;

            if (string.IsNullOrEmpty(sCodigoError)) {
                m_Log.GrabarMensaje("cErroresSinc", "SigNumLinea", "No se ha indicado un codigo de error valido", enuGravedad.EsError);
                TareaCompletadaEvent(m_Instancia, "No se ha indicado un codigo de error valido");
                throw new xvkArgumentoNuloExcepcion("cControlBDXML","SigNumLinea","No se ha indicado un codigo de error valido", "sCodigoError", m_Log);
            }

            sSql = "SELECT MAX(LINEA) FROM CONTROLSYNCD WHERE CODIGO = '" + sCodigoError + "'";
            using (DbConnection cnLite = this.cnValida()) {
                cmdComando = cnLite.CreateCommand();
                cmdComando.CommandText = sSql;

                try {
                    cnLite.Open();
                    nContador = Convert.ToInt32(cmdComando.ExecuteScalar()) + 1;
                } catch (NullReferenceException) {
                    return 1;
                } catch (Exception ex) {
                    m_Log.GrabarMensaje("Error: " + ex.Message, ex);
                    TareaCompletadaEvent(m_Instancia, ex.Message);
                    throw new xvkExcepcion("Error al consultar el numero de linea.", ex, m_Log);
                }
            }

            return nContador + 1;
    }

        /// <summary>
        /// Crea la tabla ControlSync - Cabecera de errores
        /// </summary>
        private void CrearTablaCONTROLSYNC() {
            StringBuilder sSQL = new StringBuilder(1000);
            DbCommand cmdComando;
            object Bloqueo = new object();

            sSQL.AppendLine("CREATE TABLE IF NOT EXISTS CONTROLSYNC (");
            sSQL.AppendLine("CODIGO TEXT PRIMARY KEY, ");
            sSQL.AppendLine("FECHA TEXT DEFAULT '', ");
            sSQL.AppendLine("NOMBREWS TEXT DEFAULT '', ");
            sSQL.AppendLine("XMLERROR TEXT DEFAULT '' ");
            sSQL.AppendLine(")");

            //Bloqueo
            lock (Bloqueo) {
                using (DbConnection cnLite = this.cnValida()) {
                    cmdComando = cnLite.CreateCommand();
                    cmdComando.CommandText = sSQL.ToString();

                    try {
                        cnLite.Open();
                        cmdComando.ExecuteNonQuery();
                    } catch (Exception ex) {
                        throw new xvkExcepcion("Error al crear la tabla CONTROLSYNC. Error: " + ex.Message, ex, m_Log);
                    }
                }
            }

            TareaCompletadaEvent(m_Instancia, "Se ha comprobado la tabla [CONTROLSYNC]");
    }

        /// <summary>
        /// Crea la tabla ControlSync - Cabecera de errores
        /// </summary>
        private void CrearTablaCONTROLSYNCD() {
            StringBuilder sSQL = new StringBuilder(1000);
            DbCommand cmdComando;
            object Bloqueo = new object();

            sSQL.AppendLine("CREATE TABLE IF NOT EXISTS CONTROLSYNCD (");
            sSQL.AppendLine("CODIGO TEXT DEFAULT '', ");
            sSQL.AppendLine("LINEA INTEGER, ");
            sSQL.AppendLine("FUNCION TEXT DEFAULT '', ");
            sSQL.AppendLine("MODULO TEXT DEFAULT '', ");
            sSQL.AppendLine("MENSAJE TEXT DEFAULT '', ");
            sSQL.AppendLine("PRIMARY KEY (CODIGO,LINEA) ");
            sSQL.AppendLine(")");

            //Bloqueo
            lock (Bloqueo) {
                using (DbConnection cnLite = this.cnValida()) {
                    cmdComando = cnLite.CreateCommand();
                    cmdComando.CommandText = sSQL.ToString();

                    try {
                        cnLite.Open();
                        cmdComando.ExecuteNonQuery();
                    } catch (Exception ex) {
                        throw new xvkExcepcion("Error al crear la tabla CONTROLSYNCD. Error: " + ex.Message, ex, m_Log);
                    }
                }
            }
            TareaCompletadaEvent(m_Instancia, "Se ha comprobado la tabla [CONTROLSYNCD]");
        }
        #endregion
    }
}