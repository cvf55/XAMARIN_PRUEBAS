using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using WSAndroid.SageX3WS;

namespace WSAndroid
{
    [Activity(Label = "WSAndroid", MainLauncher = false, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.cmdBoton1);
            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            //Configuramos el contexto que usaremos en la aplicacion
            CAdxCallContext adxContexto;
            CAdxWebServiceXmlCCService adxControlWS;
            CAdxParamKeyValue[] adxParametros;
            CAdxResultXml adxResultado;

            string XMLEntrada;

            adxContexto = new CAdxCallContext();
            adxControlWS = new CAdxWebServiceXmlCCService();
            adxParametros = new CAdxParamKeyValue[1];

            adxContexto.codeLang = "SPA";
            adxContexto.codeUser = "PORTAL";
            adxContexto.password = "caefe40057";
            adxContexto.poolAlias = "COFRICO";
            //adxContexto.requestConfig = "";

            //Establecemos los parametros
            adxParametros[0] = new CAdxParamKeyValue();
            adxParametros[0].key = "CLB";
            adxParametros[0].value = "PRU";

            //Hacemos la llamada al WS
            XMLEntrada = "<PARAM>";
            XMLEntrada += "<FLD NAME = " + '\u0022' + "CLB" + '\u0022' + ">PRU</FLD>";
            XMLEntrada += "<TAB ID = " + '\u0022' + "EXS0_3" + '\u0022' + ">";
            XMLEntrada += "<LIN NUM = " + '\u0022' + "2" + '\u0022' + ">";
            XMLEntrada += "<FLD NAME = " + '\u0022' + "FCY" + '\u0022' + ">0101</FLD>";
            XMLEntrada += "<FLD NAME = " + '\u0022' + "DATEXS" + '\u0022' + ">20151103</FLD>";
            XMLEntrada += "<FLD NAME = " + '\u0022' + "CODEXP" + '\u0022' + ">AUTOPISTA</FLD>";
            XMLEntrada += "<FLD NAME = " + '\u0022' + "CUR" + '\u0022' + ">EUR</FLD>";
            XMLEntrada += "<FLD NAME = " + '\u0022' + "QTY" + '\u0022' + ">1</FLD>";
            XMLEntrada += "<FLD NAME = " + '\u0022' + "ZPAFOR" + '\u0022' + ">1</FLD>";
            XMLEntrada += "<FLD NAME = " + '\u0022' + "UOM" + '\u0022' + ">UN</FLD>";
            XMLEntrada += "<FLD NAME = " + '\u0022' + "AMTPAY" + '\u0022' + ">1.0</FLD>";
            XMLEntrada += "</LIN>";
            XMLEntrada += "</TAB>";
            XMLEntrada += "</PARAM>";

            try
            {
                adxResultado = adxControlWS.modify(adxContexto, "ZWEXSAND", adxParametros, XMLEntrada);

                Toast menMensajePantalla = Toast.MakeText(this, adxResultado.resultXml, ToastLength.Long);
                menMensajePantalla.Show();
            } catch (Exception ex)
            {
                Toast menMensajePantalla = Toast.MakeText(this, ex.Message, ToastLength.Long);
                menMensajePantalla.Show();
            }
            
        }
    }
}

