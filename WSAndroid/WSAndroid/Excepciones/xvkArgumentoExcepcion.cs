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
    class xvkArgumentoExcepcion : ArgumentException {
#region "Variables"
        private ILog m_clsLog;
#endregion

#region "Contructores"
        internal xvkArgumentoExcepcion() : this("", "", "", null) { }

        internal xvkArgumentoExcepcion(string Mensaje, string Parametro, Exception ExcepcionInterna, ILog clsLog = null) : base(Mensaje, Parametro, ExcepcionInterna) {
            if (clsLog != null) {
                m_clsLog = clsLog;
                m_clsLog.GrabarMensaje(Mensaje, ExcepcionInterna);
            }
        }

        internal xvkArgumentoExcepcion(string Modulo, string Funcion, string Mensaje, string Parametro, ILog clsLog = null) : base(Mensaje, Parametro) {
            if (clsLog != null) {
                m_clsLog = clsLog;
                m_clsLog.GrabarMensaje(Modulo, Funcion, Mensaje, enuGravedad.EsError);
            }
        }
#endregion
    }
}