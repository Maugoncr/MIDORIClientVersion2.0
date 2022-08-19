using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidoriValveTest
{
    static class Program
    {
        public static string P_unit = "PSI"; //leer desde aca del archivo de configuracion (hasta etonces sera PSI)
        public static string AP_unit = "1:1"; //Se utilizara si nos es posible modificar la cuenta de pasos fuera de utilizar grados. 


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //TODO :
            //ARCHIVO 1 : configuracion de unidades. 
            //-verificar existencia de archivo de configuracion 
            //-si existe leer configuraciones y aplicarlas a los valores generales.
            //-si no existe, crear el archivo de nuevo y agregar configuraciones normales. 

            //ARCHIVO 2 almacenamiento de configurtaciones de PID. 
            //-verificar existencia de archivo de configuracion 
            //-si existe leer configuraciones y aplicarlas a los valores generales.
            //-si no existe, notificar en pantalla que no existe, e intentar obtener los actuales de la valvula. 
            //si no lo consigue notificar al usuario y pedir nueva configuracion de valores para PID y unidades


            
                Application.Run(new Midori_PV());
            
            
           
        }
    }
}
