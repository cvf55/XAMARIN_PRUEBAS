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
using System.Net;
using System.IO;

namespace WSAndroid {
    class cDropBox {
        #region "Variables privadas"
        private const string sUri = "";
        private const string TOKEN = "";
        #endregion

        #region "Contructor"

        #endregion

        #region "Metodos publicos"
        /// <summary>
        /// Crea un directorio
        /// </summary>
        public void CrearDirectorio() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sube un fichero a un directorio especificado
        /// </summary>
        public void SubirFichero() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Baja un fichero a un directorio indicado 
        /// </summary>
        public void BajarFichero() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Borrar un fichero o un directorio
        /// </summary>
        public void Borrar() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busca un directiro o un fichero indicado.
        /// </summary>
        public void Buscar(string Path,string Query, int MaxResult = 100) {
            const string sURI = "https://api.dropboxapi.com";
            //Vamos a probar

            
            HttpWebRequest Peticion =(HttpWebRequest) HttpWebRequest.Create (sURI);
            Peticion.UserAgent = "api-explorer-client";
            Peticion.ContentType = "application/json";
            Peticion.Method = "POST";
            Peticion.Headers.Add("Authorization","Bearer KfuIcaBtIaAAAAAAAAACoFpv1Gvxmy_uZHwvtl62g0I59AOxs8HPmgbz2avgNlwM");

            //Obtenemos la respuesta del servidor
            try {
                StreamReader Respuesta = new StreamReader(Peticion.GetRequestStream());
                StringBuilder Cadena = new StringBuilder(Respuesta.ReadToEnd());
                Respuesta.Close();

                //Hay que deserializar la cadena ya que viene en JSON
                Toast t = Toast.MakeText(Application.Context, Cadena.ToString(), ToastLength.Long);
                t.Show();

                //Guardamos la respuesta del servidor
                WebResponse Response = Peticion.GetResponse();
                Respuesta = new StreamReader(Response.GetResponseStream());
                Cadena.Clear();
                Cadena.Append(Respuesta.ReadToEnd());

                //Hay que deserializar la cadena ya que viene en JSON
                Toast s = Toast.MakeText(Application.Context, Cadena.ToString(), ToastLength.Long);
                s.Show();
            } catch (Exception ex) {
                Toast toast = Toast.MakeText(Application.Context, ex.Message, ToastLength.Long);
                toast.Show();
            } 
        }

        #endregion

        #region "Metodos privados"

        #endregion

    }
}