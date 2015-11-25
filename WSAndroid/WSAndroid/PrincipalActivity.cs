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

namespace WSAndroid
{
    [Activity(Label = "PrincipalActivity", MainLauncher = true, Icon = "@drawable/icon")]
    public class PrincipalActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Establecemos el layout en esta activity
            SetContentView(Resource.Layout.Principal);

            //Enlazamos los eventos con los botones
            Button button1 = FindViewById<Button>(Resource.Id.Boton1);
            button1.Click += Boton1_Click;

            Button button2 = FindViewById<Button>(Resource.Id.Boton2);
            button2.Click += Boton2_Click;

            Button button3 = FindViewById<Button>(Resource.Id.Boton3);
            button3.Click += Boton3_Click;

            Button button4 = FindViewById<Button>(Resource.Id.Boton4);
            button4.Click += Boton4_Click;

            Button button5 = FindViewById<Button>(Resource.Id.Boton5);
            button5.Click += Boton5_Click;

            Button button6 = FindViewById<Button>(Resource.Id.Boton6);
            button6.Click += Boton6_Click;
        }

        public void Boton1_Click(object sender, EventArgs args)
        {
            Toast menMensaje = Toast.MakeText(this, "Estado de la conexion a Inet", ToastLength.Long);
            menMensaje.Show();

            StartActivity(typeof(ConexionInet));
        }

        public void Boton2_Click(object sender, EventArgs args)
        {
            Toast menMensaje = Toast.MakeText(this, "Bien!!. Boton 2 pulsado", ToastLength.Long);
            menMensaje.Show();
        }

        public void Boton3_Click(object sender, EventArgs args)
        {
            Toast menMensaje = Toast.MakeText(this, "Bien!!. Boton 3 pulsado", ToastLength.Long);
            menMensaje.Show();
        }


        public void Boton4_Click(object sender, EventArgs args)
        {
            Toast menMensaje = Toast.MakeText(this, "Bien!!. Boton 4 pulsado", ToastLength.Long);
            menMensaje.Show();
        }


        public void Boton5_Click(object sender, EventArgs args)
        {
            Toast menMensaje = Toast.MakeText(this, "Bien!!. Boton 5 pulsado", ToastLength.Long);
            menMensaje.Show();
        }

        public void Boton6_Click(object sender, EventArgs args)
        {
            Toast menMensaje = Toast.MakeText(this, "Bien!!. Boton 6 pulsado", ToastLength.Long);
            menMensaje.Show();
        }
    }
}