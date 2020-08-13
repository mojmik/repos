using System;

namespace PrikladNaradi
{
    class Program
    {
        static void Main(string[] args)
        {
            Bedna b = new Bedna(10000);
            b.VlozNaradi(new Kladivo("Kladívko",1200));
            b.VlozNaradi(new Kladivo("Velké kladivo", 3000,true));
            b.VlozNaradi(new Kladivo("Bourací kladivo", 4000,true));
            b.VlozNaradi(new Sroubovak("Šroubovák Philips", 800));
            b.VlozNaradi(new Pila("Zrezivělá pila", 2500));
            b.VlozNaradi(new ElektrickySroubovak("Elektrický šroubovák Bosh", 1800,100));
            foreach (Naradi n in b.GetNaradi())
            {
                Console.WriteLine(n.Pracuj());
            }
            b.VypisNaradi();
            b.VahaKladiv();
        }
    }
}
