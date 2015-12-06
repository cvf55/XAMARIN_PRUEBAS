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
using Mono.Data.Sqlite;
using System.Data.Common;

namespace WSAndroid {
    /// <summary>
    /// Control de las bases de datos locales en SQL
    /// </summary>
    interface IControlBD {
        /// <summary>
        /// Devuelve una conexion valida.
        /// </summary>
        /// <returns>Conexion valida</returns>
        DbConnection cnValida();

        /// <summary>
        /// Devuelve un DataAdapter
        /// </summary>
        /// <returns>DataAdapter</returns>
        DbDataAdapter DevolverDataAdapter(string SQL);

        /// <summary>
        /// Devuelve true si la conexion es valida
        /// </summary>
        /// <returns>true o false</returns>
        bool ComprobarConexion();

        /// <summary>
        /// Crea una base de datos vacia.
        /// </summary>
        void CrearBD();

        /// <summary>
        /// Limpia la base de datos de registros innecesarios. Se utiliza para mantener el menor tamaño posible de la base de datos.
        /// </summary>
        void LimpiarBD(int DiasAConservar = 7);
    }
}