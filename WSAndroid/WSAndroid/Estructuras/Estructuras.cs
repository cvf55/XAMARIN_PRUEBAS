/// <summary>
/// Estructura que informa de un error en la sincronizacion. Se rellena en la clase cControlBDXML
/// </summary>
internal struct struErroresSinc {
    public string Codigo;
    public System.DateTime Fecha;
    public string Modulo;
    public string Funcion;
    public string Mensaje;
    public string XMLError;
    public string NombreWS;
    }