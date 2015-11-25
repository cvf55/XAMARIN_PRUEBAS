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
using Android.Telephony;

namespace WSAndroid
{
    [Activity(Label = "ConexionInet")]
    public class ConexionInet : Activity
    {
        //Controles
        private TextView txtResulConexion;
        private TextView txtResulTipoConexion;
        private TextView txtResulNombreConexion;
        private TextView txtResulFuerzaSenal;
        private TextView txtResulSubida;
        private TextView txtResulBajada;
        private Button cmdComenzar;

        private cTestInet m_clsConexionInet;

        #region "Metodos privados"
        private void MostrarResutladoPantalla() {
            txtResulConexion.Text = m_clsConexionInet.ConexionInet.ToString();
            txtResulTipoConexion.Text = m_clsConexionInet.TipoConexion.ToString();
            txtResulSubida.Text = m_clsConexionInet.VelocidadSubidaReal.ToString();
            txtResulBajada.Text = m_clsConexionInet.VelocidadBajadaReal.ToString();
        }
        #endregion

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Establecemos el layout en esta activity
            SetContentView(Resource.Layout.ConexionInet);

            //Enlazamos los controles.
            this.txtResulConexion = FindViewById<TextView>(Resource.Id.txtResulConexion);
            this.txtResulTipoConexion = FindViewById<TextView>(Resource.Id.txtResulTipoConexion);
            this.txtResulSubida = FindViewById<TextView>(Resource.Id.txtResulSubida);
            this.txtResulBajada = FindViewById<TextView>(Resource.Id.txtResulBajada);
            this.txtResulFuerzaSenal = FindViewById<TextView>(Resource.Id.txtResulFuerzaSenalMobile);
            this.txtResulNombreConexion = FindViewById<TextView>(Resource.Id.txtResulNombreConexion);
            this.cmdComenzar = FindViewById<Button>(Resource.Id.cmdComenzar);
            this.cmdComenzar.Click += CmdComenzar_Click;

            //Realizamos la prueba de la conexion a Inet
            m_clsConexionInet = new cTestInet(this);
            m_clsConexionInet.ComprobarConexionINetEvent += ComprobarConexionINetEvent;
            m_clsConexionInet.DeterminarTipoConexionEvent += DeterminarTipoConexionEvent;
            m_clsConexionInet.TestVelocidadEvent += TestVelocidadEvent;
            m_clsConexionInet.SignalStrengthChangedEvent += SignalStrengthChangedEvent;

            m_clsConexionInet.ComprobarConexionINet();
            m_clsConexionInet.ObtenerTipoConexion();
        }

        private void CmdComenzar_Click(object sender, EventArgs e) {
            Toast t = Toast.MakeText(this, "Realizando el test de velocidad. Espere...", ToastLength.Long);
            t.Show();

            m_clsConexionInet.RealizarTestVelocidad();

            Toast s = Toast.MakeText(this, "Test de velocidad realizado. Espere...", ToastLength.Long);
            s.Show();
        }

        /// <summary>
        /// Evento que se ejecuta cuando se termina la comprobacion de Inet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ComprobarConexionINetEvent(object sender, EventArgs e) {
            //Mostramos los datos en pantalla.
            txtResulConexion.Text = m_clsConexionInet.ConexionInet.ToString();
            txtResulTipoConexion.Text = m_clsConexionInet.TipoConexion.ToString();
            //this.MostrarResutladoPantalla();
        }

        /// <summary>
        /// Evento que se ejecuta cuando se termina la comprobacion del tipo de conexion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DeterminarTipoConexionEvent(object sender, EventArgs e) {
            //Mostramos los datos en pantalla.
            this.txtResulConexion.Text = m_clsConexionInet.ConexionInet.ToString();
            this.txtResulTipoConexion.Text = m_clsConexionInet.TipoConexion.ToString();
            this.txtResulNombreConexion.Text = m_clsConexionInet.NombreConexion;
        }

        /// <summary>
        /// Evento que se ejecuta cuando se termina el test de velocidad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TestVelocidadEvent(object sender, EventArgs e) {
            //Mostramos los datos en pantalla.
            txtResulSubida.Text = m_clsConexionInet.VelocidadSubidaReal.ToString();
            txtResulBajada.Text = m_clsConexionInet.VelocidadBajadaReal.ToString();
            //this.MostrarResutladoPantalla();
        }

        /// <summary>
        /// Evento que se ejecuta cuando se ha terminado de comprobar la fuerza de la señal
        /// </summary>
        /// <param name="Fuerza"></param>
        protected void SignalStrengthChangedEvent() {
            txtResulFuerzaSenal.Text = m_clsConexionInet.FuerzaSenalMobile.ToString();
        }
    }
}