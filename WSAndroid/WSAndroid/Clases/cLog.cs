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
    internal class cLog : ILog {
        #region "Variables"
        private bool m_ModoDebug; //Activa el modo debug
        private IControlBD m_BaseDatos; //Base de datos para almacenar el log
        private IEnvioMail m_EnvioMail; //Clase para el envio de email.
        #endregion

        #region "Propiedades"
        public bool ModoDebug {
            get { return m_ModoDebug; }

            set { m_ModoDebug = value; }
        }
        #endregion

        #region "Contructores"
        public cLog(IControlBD BaseDatos, IEnvioMail Email = null) {
            m_BaseDatos = BaseDatos;
            m_EnvioMail = Email;
        }
        #endregion

        #region "Metodos publicos"
        public void GrabarMensaje(string Mensaje) {
            GrabarMensaje("", "", 0, Mensaje, Mensaje, "", enuGravedad.EsInformativo, false, "");
        }

        public void GrabarMensaje(string Mensaje, Exception Excepcion) {
            string sTextoLinea; //Guardamos el numero de linea de la pila de llamadas. No viene siempre asi que lo tenemos que buscar
            int nLinea; //Guardamos el numero de linea en INT

            //TODO: Probar. esto a ver si es posible 
            //Dim st As StackTrace = New StackTrace(ex, True)

            // Recorrermos la colección de los marcos de la pila actual
            //For Each sf As StackFrame In st.GetFrames

            //    Console.WriteLine("Archivo: {0}, Nombre Método: {1}, Número de Línea: {2}", _
            //                      sf.GetFileName(), sf.GetMethod().Name, sf.GetFileLineNumber())
            //Next

            if (!String.IsNullOrEmpty(Excepcion.StackTrace)) {
                if (Excepcion.StackTrace.IndexOf("línea") > 0)
                    sTextoLinea = Excepcion.StackTrace.Substring(Excepcion.StackTrace.IndexOf("línea") + 6);
                else if (Excepcion.StackTrace.IndexOf("line") > 0)
                    sTextoLinea = Excepcion.StackTrace.Substring(Excepcion.StackTrace.IndexOf("line") + 5);
                else
                    sTextoLinea = "0";
            } else
                sTextoLinea = "0";


            //Vamos a pasar el texto de la linea a numero
            if (sTextoLinea.IndexOf(" ") > 0) {
                //nLinea = Convert.ToInt32(sTextoLinea.Substring(sTextoLinea.IndexOf(" ") + 1, sTextoLinea.Length - (sTextoLinea.IndexOf(" ") + 1)))
                nLinea = Convert.ToInt32(sTextoLinea.Substring(0, sTextoLinea.IndexOf(" ")));
            } else
                nLinea = 0;

            GrabarMensaje(Excepcion.TargetSite.ReflectedType.FullName, Excepcion.TargetSite.ToString(), nLinea, Mensaje, Excepcion.Message, Excepcion.GetType().FullName, enuGravedad.EsError, false, "");
        }

        public void GrabarMensaje(string Modulo, string Funcion, string Mensaje, enuGravedad Gravedad) {
            GrabarMensaje(Modulo, Funcion, 0,Mensaje, "", "", enuGravedad.EsInformativo, false, "");
        }

        public void GrabarMensajeDebug(string Modulo, string Funcion, string Mensaje1, string Mensaje2 = "") {
            GrabarMensaje(Modulo, Funcion, 0, Mensaje1, Mensaje2, "", enuGravedad.EsDebug, false, "");
        }
        #endregion

        /// <summary>
        /// Graba un mensaje en la tabla de logs
        /// </summary>
        /// <param name="sModulo">Modulo donde se ha producido el error</param>
        /// <param name="sFuncion">Funcion</param>
        /// <param name="nLinea">Nº de linea</param>
        /// <param name="sMensaje1">Mensaje 1</param>
        /// <param name="sMensaje2">Mensaje 2</param>
        /// <param name="Gravedad">Nivel de gravedad</param>
        /// <param name="bEmailEnviado">Graba si el email ha sido enviado o no</param>
        /// <param name="sDestinatarios">Indica los destinatarios del email.</param>
        #region "Metodos privados"
        private void GrabarMensaje(string sModulo, string sFuncion, int nLinea, string sMensaje1, string sMensaje2, string NombreExcepcion, enuGravedad Gravedad, bool bEmailEnviado, string sDestinatarios) {

            //Si es un mensaje de debug y no esta activo el modo debugm no lo grabamos
            if (Gravedad == enuGravedad.EsDebug && !this.ModoDebug)
                return;

            GrabarLogTabla(m_BaseDatos, sModulo, sFuncion, nLinea, sMensaje1, sMensaje2, NombreExcepcion, Gravedad, bEmailEnviado, sDestinatarios);
        }

        private bool GrabarLogTabla(IControlBD ControlBD, string sModulo, string sFuncion, int nLinea, string sMensaje1, string sMensaje2, string NombreExcepcion,
                                enuGravedad enuGravedad, bool bEmailEnviado, string sDestinatarios) {

            DbCommand cmdComando;
            StringBuilder sSql = new StringBuilder(1000);

            //Intenta grabar un mensaje en la tabla de Log
            sSql.Clear();
            sSql.Append("INSERT INTO ZLOG ");
            sSql.Append("(MODULO_0,FUNCION_0,LINEA_0,MENSAJE1_0,MENSAJE2_0,NOMBREEXCEPCION,GRAVEDAD_0,FECHA_0,HORA_0,EMAIL_0,DESTINATARIO_0) VALUES (");
            sSql.Append("'" + sModulo.Substring(0, 100) + "','" + sFuncion.Substring(0, 100) + "'," + nLinea.ToString() + ",'" + sMensaje1.Substring(0, 250) + "','" + sMensaje2.Substring(0, 250) + "',");
            sSql.Append("'" + NombreExcepcion + "'," + enuGravedad + ",'" + DateTime.Today.ToString("yyyyMMdd") + "','" + DateTime.Now.ToString("HH:mm:ss:ff") + "','" + (bEmailEnviado ? "S" : "N") + "','" + sDestinatarios + "')");

            using (DbConnection cnSqlite = ControlBD.cnValida()) {
                cmdComando = cnSqlite.CreateCommand();
                cmdComando.CommandText = sSql.ToString();
                try {
                    cnSqlite.Open();
                    cmdComando.ExecuteNonQuery();
                } catch {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}