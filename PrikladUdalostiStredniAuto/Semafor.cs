using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladUdalostiStredniAuto
{
    public class Semafor
    {
        public enum Svetlo { Zelena, Oranzova, Cervena };
        private Svetlo sviti;
        public Svetlo Sviti
        {
            get
            {
                return sviti;
            }
            set
            {
                sviti = value;

            }
        }
        public event EventHandler Ehandler;

        public Semafor()
        {
            prepniSvetlo();
        }
        public void prepniSvetlo(Svetlo s = Svetlo.Zelena)
        {
            Sviti = s;
            Console.WriteLine("Sviti " + Sviti);
            PriPrepnuti(EventArgs.Empty);
        }
        protected void PriPrepnuti(EventArgs e)
        {
            Ehandler?.Invoke(this, e);
        }
    }
}
