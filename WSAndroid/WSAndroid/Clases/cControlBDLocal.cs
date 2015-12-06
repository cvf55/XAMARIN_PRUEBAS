using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Mono.Data.Sqlite;

namespace WSAndroid  {
    internal abstract class cControlBDLocal : IControlBD {
        #region "Variables"
        private ILog m_Log; //Clase que controla las insercciones en el log
        //private DbConnection m_cnSqlite; //Base de datos con la que vamos a trabajar.
        private string m_RutaBD; //Ruta de la base de datos.
        private string m_NombreBD; //Nombre de la base de datos.
        #endregion

        #region "Propiedades"
        /// <summary>
        /// Devuelve la ruta completa de la base de datos que se crea
        /// </summary>
        public string RutaCompletaBD {
            get { return Path.Combine(m_RutaBD, m_NombreBD); }
        }

        //Control del log
        protected ILog Log {
            get { return m_Log; }
        }
        #endregion

        #region "Eventos y delegados"
        public delegate void TareaCompletadaHandler(cControlBDLocal sender, string Mensaje);
        public virtual event TareaCompletadaHandler TareaCompletadaEvent;
        #endregion

        #region "Constructor"
        public cControlBDLocal(string NombreBD,ILog cLog = null) {
            m_NombreBD = NombreBD;
            m_Log = cLog;

            if (string.IsNullOrEmpty(NombreBD)) {
                throw new xvkArgumentoNuloExcepcion("cControlBDLocal", "New", "No se ha indicado la nombre de la BD.", "NombreBD");
            }

            //Obtenemos la ruta de la base de datos. Recordar que no tenemos permisos para todos los directorios, asi que lo grabamos en el espacio personal que nos da el OS.
            m_RutaBD = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), m_NombreBD);

            //Creamos la base de datos, si no esta creada. 
            this.CrearBD();

            //Limpiamos los registros antiguos para mantener la base de datos lo mas pequeña posible.
            this.LimpiarBD();
        }
        #endregion

        #region "Metodos privados"

        #endregion

        #region "Metodos publicos"
        /// <summary>
        /// Devuelve una conexion valida segun los parametros indicados.
        /// </summary>
        /// <returns></returns>
        public DbConnection cnValida() {
            return new SqliteConnection("Data Source=" + this.RutaCompletaBD + ";Version=3;");
        }

        /// <summary>
        /// Comprueba que la conexion especificada es valida
        /// </summary>
        /// <returns></returns>
        public bool ComprobarConexion() {
            try {
                using (DbConnection cnSqlite = this.cnValida()) {
                    cnSqlite.Open();
                    return true;
                }
            } catch {
                return false;
            }
        }
        
        /// <summary>
        /// Comprueba si existe la base de datos y si no existe la crea. Tambien crea las tablas necesarias.
        /// </summary>
        public void CrearBD() {
            m_Log.GrabarMensaje("Comprobando la base de datos local");
            TareaCompletadaEvent(this, "Iniciando la creacion de la base de datos local");

            //Comprobamos si ya existe la base de datos.
            if (File.Exists(Path.GetDirectoryName(this.RutaCompletaBD)))
                return;

            this.CrearTablas();

            m_Log.GrabarMensaje("La comprobacion de la base de datos local ha terminado");
            TareaCompletadaEvent(this, "Finalizada la creacion de la base de datos local");
        }

        /// <summary>
        /// Crea las tablas en la base de datos. Es abstracto para que cada implementacion cree sus propias tablas
        /// </summary>
        abstract public void CrearTablas();

        /// <summary>
        /// Limpia la base de datos de registros innecesarios. Se utiliza para mantener el menor tamaño posible de la base de datos.
        /// </summary>
        abstract public void LimpiarBD(int DiasAConservar = 7);

        /// <summary>
        /// Devuelve un DataAdapter de la conexion especificada
        /// </summary>
        /// <returns></returns>
        public DbDataAdapter DevolverDataAdapter(string SQL) {
            return new SqliteDataAdapter(SQL, (SqliteConnection) this.cnValida());
        }
        #endregion
    }
}