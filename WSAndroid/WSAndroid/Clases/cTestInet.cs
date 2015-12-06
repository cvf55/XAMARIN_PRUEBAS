using System;
using System.IO;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.Telephony;
using System.IO.IsolatedStorage;

/// <summary>
/// Clase para realizar el test de internet
/// </summary>
namespace WSAndroid {
    class cTestInet
    {
        #region "Variables privadas"
        private bool m_ConexionINet;
        private enumTipoConexion m_TipoConexion;
        private double m_VelocidadSubidaReal;
        private double m_VelocidadSubidaTeorica;
        private double m_VelocidadBajadaReal;
        private double m_VelocidadBajadaTeorica;
        private int m_FuerzaSenalMobile;
        private string m_NombreConexion;

        private Context m_Contexto;
        private TelephonyManager m_TelefonoManager;
        private ConnectivityManager m_ConnectivityManager;
        private WifiManager m_WifiManager;

        private GsmSignalStrengthListener m_GsmSignalStrengthListener;
        #endregion

        #region "Propiedades"
        /// <summary>
        /// Devuelve true si hay conexion, false si no.
        /// </summary>
        public bool ConexionInet {
            get { return m_ConexionINet; }
        }

        /// <summary>
        /// Devuelve el tipo de conexion
        /// </summary>
        public enumTipoConexion TipoConexion {
            get { return m_TipoConexion; }
        }

        /// <summary>
        /// Velocidad de subida que hemos medido en el momento actual.
        /// </summary>
        public double VelocidadSubidaReal {
            get { return m_VelocidadSubidaReal; }
        }

        /// <summary>
        /// Velocidad de subida teorica segun el tipo de conexion.
        /// </summary>
        public double VelocidadSubidaTeorica {
            get { return m_VelocidadSubidaTeorica; }
        }

        /// <summary>
        /// Velocidad de bajada que hemos medido en el momento actual.
        /// </summary>
        public double VelocidadBajadaReal {
            get { return m_VelocidadBajadaReal; }
        }

        /// <summary>
        /// Velocidad de bajada teorica segun el tipo de conexion.
        /// </summary>
        public double VelocidadBajadaTeorica {
            get { return m_VelocidadBajadaTeorica; }
        }

        /// <summary>
        /// Fuerza de la señal de la cobertura mobile.
        /// </summary>
        public double FuerzaSenalMobile {
            get { return m_FuerzaSenalMobile; }
        }

        /// <summary>
        /// Nombre de la conexion, si esta conectado a una red de datos moviles. Por ejemplo GPRS, EDGE, etc
        /// </summary>
        public string NombreConexion {
            get { return m_NombreConexion; }
        }
        #endregion

        #region "Constructor"
        public cTestInet(Context c) {
            m_Contexto = c;

            m_ConexionINet = false;
            m_TipoConexion = enumTipoConexion.SINCONEXION;
            m_NombreConexion = string.Empty;
            m_VelocidadSubidaReal =  0d;
            m_VelocidadSubidaTeorica = 0d;
            m_VelocidadBajadaReal = 0d;
            m_VelocidadBajadaTeorica = 0d;

            m_GsmSignalStrengthListener = new GsmSignalStrengthListener();
            m_GsmSignalStrengthListener.SignalStrengthChanged += HandleSignalStrengthChanged;

            m_TelefonoManager = (TelephonyManager) m_Contexto.GetSystemService(Context.TelephonyService);
            m_TelefonoManager.Listen(m_GsmSignalStrengthListener, PhoneStateListenerFlags.SignalStrengths);

            m_ConnectivityManager = (ConnectivityManager) m_Contexto.GetSystemService(Context.ConnectivityService);
            m_WifiManager = (WifiManager) m_Contexto.GetSystemService(Context.WifiService);
        }
        #endregion

        #region "Eventos y delegados"
        //Definimos el delegado
        public delegate void TestInetEventHandler(object sender, EventArgs e);
        public delegate void SignalStrengthChangedHandler();

        /// <summary>
        /// Evento que indica que ha terminado el test de velocidad
        /// </summary>
        public event TestInetEventHandler TestVelocidadEvent;
    
        /// <summary>
        /// Evento que indica que ha terminado de comprobar la conexion a internet
        /// </summary>
        public event TestInetEventHandler ComprobarConexionINetEvent;

        /// <summary>
        /// Evento que indica que ha terminado de determinar el tipo de conexion
        /// </summary>
        public event TestInetEventHandler DeterminarTipoConexionEvent;

        public event SignalStrengthChangedHandler SignalStrengthChangedEvent;
        #endregion

        #region "Metodos publicos"
        /// <summary>
        /// Realiza un test de velocidad completo. Calcula la subida y la bajada. Lanza el evento ComprobarConexionINetEvent cuando ha terminado
        /// </summary>
        public void RealizarTestVelocidad()
        {
            string sRutaFicheroGenerado = string.Empty;
            //double nVelSubida = 0d;
            //double nVelBajada = 0d;

            //1 - Vamos a generar un fichero de una longitud determinada
            sRutaFicheroGenerado = this.GenerarFicheroPruebas(512);

            //2 - Vamos a subir el fichero para comprobar la velocidad de subida
            //nVelSubida = this.SubirFicheroTest(sRutaFicheroGenerado);

            //3 - Bajamos el fichero para comprobar la velocidad de bajada
            //nVelBajada = this.BajarFicheroTest(sRutaFicheroGenerado);

            //Hemos terminado. Lanzamos el evento.
            ComprobarConexionINetEvent(this, EventArgs.Empty);
        }

        /// <summary>
        /// Comprueba la conexion a internet. Lanza el evento TestVelocidadEvent cuando ha acabado
        /// <param name="context">Contexto</param>
        /// </summary>
        public void ComprobarConexionINet()
        {
            //Determinamos si hay conexion
            ConnectivityManager connectivityManager = (ConnectivityManager) m_Contexto.GetSystemService(Context.ConnectivityService);
            NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
            m_ConexionINet = (activeConnection != null) && activeConnection.IsConnected;

            if (m_ConexionINet == true)
                m_TipoConexion = enumTipoConexion.CONEXIONSINDETERMINAR;
            else
                m_TipoConexion = enumTipoConexion.SINCONEXION;

            //Hemos terminado. Lanzamos el evento.
            ComprobarConexionINetEvent(this, EventArgs.Empty);
        }

        /// <summary>
        /// Determina el tipo de conexion a internet. Lanza el evento TextVelocidadEvent cuando ha acabado
        /// </summary>
        /// <param name="context">Contexto</param>
        public void ObtenerTipoConexion()
        {
            //Vamos a determinar el tipo de conexion que tiene el dispositivo. Solo podemos determinar WIFI o MOBILE
            if (HayWifi())
            {
                m_TipoConexion = enumTipoConexion.WIFI;
                m_NombreConexion = this.ObtenerSSIDWIFI();
                m_ConexionINet = true;
            }
            else if (HayMobile())
            {
                m_TipoConexion = enumTipoConexion.MOBILE;
                m_NombreConexion = this.ObtenerNombreConexionDatos();
                m_ConexionINet = true;
            }
            else
            {
                m_TipoConexion = enumTipoConexion.SINCONEXION;
                m_NombreConexion = "";
                m_ConexionINet = false;
            }

            //Lanzamos el evento para notificar a la clase cliente
            DeterminarTipoConexionEvent(this, EventArgs.Empty);
        }
        #endregion

        #region "Metodos privados"
        /// <summary>
        /// Determina si hay conexion WIFI activo en el dispositivo.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool HayWifi()
        {
            //Primero comprobamos si hay conexion a internet
            ComprobarConexionINet();

            if (!this.ConexionInet) {
                return false;
            }

            NetworkInfo wifiInfo = m_ConnectivityManager.GetNetworkInfo(ConnectivityType.Wifi);
            if (wifiInfo.IsConnected) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Determina si hay conexion de datos activa en el dispositivo.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool HayMobile() {
            //Primero comprobamos si hay conexion a internet
            ComprobarConexionINet();

            if (!this.ConexionInet) {
                return false;
            }

            NetworkInfo mobileInfo = m_ConnectivityManager.GetNetworkInfo(ConnectivityType.Mobile);
            if (mobileInfo.IsConnected) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Obtiene el nombre de la conexion de datos. Por ejemplo GPRS, EDGE, etc
        /// </summary>
        /// <returns>Cadena con el nombre de la conexion</returns>
        private string ObtenerNombreConexionDatos() {
            string sResultado = string.Empty; 

            if (m_TelefonoManager != null)
                sResultado = m_TelefonoManager.NetworkType.ToString();

            return sResultado;
        }

        /// <summary>
        /// Obtiene el SSID de la red wifi en la que este conectado
        /// </summary>
        /// <returns>SSID de la red WIFI</returns>
        private string ObtenerSSIDWIFI() {
            string sSSID = string.Empty;

            if (m_WifiManager != null) {
                sSSID = m_WifiManager.ConnectionInfo.SSID;
            }

            return sSSID;
        }

        /// <summary>
        /// Genera un fichero de pruebas aleatorio con el tamaño indicado y devuelve su ruta completa
        /// </summary>
        /// <param name="TamañoFicheroKb">Tamaño del fichero a generar en Kb</param>
        /// <returns>Ruta completa del fichero generado</returns>
        private string GenerarFicheroPruebas(int TamañoFicheroKb) {
            IsolatedStorageFile ArchivosPersonales;
            StringBuilder Buffer;
            const string sNombreFichero = "Test.dat";
            
            //Creamos los directorios para almacenar el fichero temporal
            string sRoot = Application.Context.GetText(Resource.String.ApplicationName);
            string sSubDir = Path.Combine(sRoot, "TestVelocidad");
            string sRutaCompleta = Path.Combine(sSubDir, sNombreFichero);

            if (TamañoFicheroKb <= 0) {
                return string.Empty;
            }

            using (ArchivosPersonales = IsolatedStorageFile.GetUserStoreForApplication()) {

                //Comprobamos si el fichero existe
                if (!ArchivosPersonales.FileExists(sRutaCompleta)) {
                    //Creamos los directorios necesarios. 
                    ArchivosPersonales.CreateDirectory(sRoot);
                    ArchivosPersonales.CreateDirectory(sSubDir);

                    //Creamos un fichero con la longitud indicada
                    Buffer = new StringBuilder(TamañoFicheroKb * 1024);
                    for (int i = 0; i < TamañoFicheroKb * 1024; i++) {
                        Buffer.Append('a');
                    }

                    //Lo grabamos en la ruta indicada
                    using (StreamWriter FicEscritura = new StreamWriter(ArchivosPersonales.CreateFile(sRutaCompleta))) {
                        FicEscritura.Write(Buffer.ToString());
                        FicEscritura.Flush();
                    }
                }
            }
            return sRutaCompleta;
        }

        private double BajarFicheroTest(string sRutaFicheroGenerado) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Realizar una prueba de subida de ficheros a la red. 
        /// </summary>
        /// <param name="sRutaFicheroGenerado">Fichero que usaremos para realizar la prueba de subida</param>
        /// <returns></returns>
        private double SubirFicheroTest(string sRutaFicheroGenerado) {
            //Usaremos el servidor de DROPBOX para realizar el test de subir ficheros.
            //Esto es una prueba.
            cDropBox clsDropBox = new cDropBox();
            clsDropBox.Buscar("", "");

            return 0d;
        }

        /// <summary>
        /// Se lanza cuando se produce un cambio en la fuerza de la señal.
        /// </summary>
        /// <param name="strength">Fuerza de la señal mobile</param>
        private void HandleSignalStrengthChanged(int strength) {
            m_FuerzaSenalMobile = strength;

            //Lanzamos el evento personalizado
            SignalStrengthChangedEvent();
        }
        #endregion
    }

    #region "Enumeraciones"
    enum enumTipoConexion
    {
        SINCONEXION = 0,
        CONEXIONSINDETERMINAR = 1,
        WIFI = 2,
        MOBILE = 3
    }
    #endregion
}