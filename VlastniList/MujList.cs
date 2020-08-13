using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VlastniList
{
    class MujList
    {
       public Object[] objPole;
       public MujList()
        {
            objPole = new object[10];
            Pocet = 0;
        }

       public int Pocet { get; set; }
       public void Pridej(object o)
        {
            if (Pocet >= objPole.Count())
            {
                Object[] objPole2 = new Object[objPole.Count()*2];
                for (int n=0;n<Pocet;n++)
                {
                    objPole2[n] = objPole[n];
                }
                objPole = objPole2;
            } 
            objPole[Pocet] = o;
            Pocet++;
        }

        ///indexer
        ///
        public object this[int index]
        {
            get { return objPole[index]; /* return the specified index here */ }
            set { objPole[index]=value; /* set the specified index to value here */ }
        }

    }
}
