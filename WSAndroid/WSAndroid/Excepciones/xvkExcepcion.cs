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

namespace WSAndroid {
    class xvkExcepcion : Exception {
#region "Variables"
        private ILog m_clsLog;
        #endregion

        #region "Contructores"
        internal xvkExcepcion() : this("", "", "", null) { }

        internal xvkExcepcion(string Mensaje, Exception ExcepcionInterna, ILog clsLog = null) : base(Mensaje, ExcepcionInterna) {

            if (clsLog != null) {
                m_clsLog = clsLog;
                m_clsLog.GrabarMensaje(Mensaje, ExcepcionInterna);
                }
        }

        internal xvkExcepcion(string Modulo, string Funcion, string Mensaje, ILog clsLog = null) : base(Mensaje) {
            if (clsLog != null) {
                m_clsLog = clsLog;
                m_clsLog.GrabarMensaje(Modulo, Funcion, Mensaje, enuGravedad.EsError);
            }
        }
#endregion
    }
}